﻿using KDistanceMachine.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Model;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace KDistanceMachine.Devices
{
	public class HCS04 : ISensor<Length>
	{
		private readonly int _echo;
		private readonly int _trigger;
		private GpioController _controller;
		private bool _shouldDispose;
		private Stopwatch _timer = new Stopwatch();

		private long _lastMeasurment = 0;

		/// <summary>
		/// Creates a new instance of the HC-SCR04 sonar.
		/// </summary>
		/// <param name="gpioController">GPIO controller related with the pins</param>
		/// <param name="triggerPin">Trigger pulse input.</param>
		/// <param name="echoPin">Trigger pulse output.</param>
		/// <param name="shouldDispose">True to dispose the Gpio Controller</param>
		public HCS04(GpioController? gpioController, int triggerPin, int echoPin, bool shouldDispose = true)
		{
			_shouldDispose = shouldDispose || gpioController is null;
			_controller = gpioController ?? new();
			_echo = echoPin;
			_trigger = triggerPin;

			if (_echo != _trigger)
			{
				// In case the echo and trigger pins are different
				_controller.OpenPin(_echo, PinMode.Input);
			}

			_controller.OpenPin(_trigger, PinMode.Output);

			_controller.Write(_trigger, PinValue.Low);

			// Call Read once to make sure method is JITted
			// Too long JITting is causing that initial echo pulse is frequently missed on the first run
			// which would cause unnecessary retry
			if (_echo != _trigger)
			{
				_controller.Read(_echo);
			}
		}

		/// <summary>
		/// Creates a new instance of the HC-SCR04 sonar.
		/// </summary>
		/// <param name="triggerPin">Trigger pulse input.</param>
		/// <param name="echoPin">Trigger pulse output.</param>
		/// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
		public HCS04(int triggerPin, int echoPin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
			: this(new GpioController(pinNumberingScheme), triggerPin, echoPin)
		{
		}

		/// <summary>
		/// Try to gets the current distance, , usual range from 2 cm to 400 cm
		/// </summary>
		/// <param name="result">Length</param>
		/// <returns>True if success</returns>
		public async Task<Length?> TryGetValue(object? data)
		{
			// Time when we give up on looping and declare that reading failed
			// 100ms was chosen because max measurement time for this sensor is around 24ms for 400cm
			// additionally we need to account 60ms max delay.
			// Rounding this up to a 100 in case of a context switch.
			long hangTicks = DateTime.UtcNow.Ticks + 100 * TimeSpan.TicksPerMillisecond;
			_timer.Reset();

			// Measurements should be 60ms apart, in order to prevent trigger signal mixing with echo signal
			// ref https://components101.com/sites/default/files/component_datasheet/HCSR04%20Datasheet.pdf
			while (DateTime.UtcNow.Ticks - _lastMeasurment < 60 * TimeSpan.TicksPerMillisecond)
			{
				await Task.Delay(TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _lastMeasurment));
			}

			_lastMeasurment = DateTime.UtcNow.Ticks;

			if (_trigger == _echo)
			{
				// Set back to output
				_controller.SetPinMode(_trigger, PinMode.Output);
			}

			// Trigger input for 10uS to start ranging
			_controller.Write(_trigger, PinValue.High);
			//DelayHelper.DelayMicroseconds(10, true);
			await Task.Delay(10);
			_controller.Write(_trigger, PinValue.Low);
			// In case we are using the same pin, switch it to input
			if (_trigger == _echo)
			{
				_controller.SetPinMode(_trigger, PinMode.Input);
			}

			// Wait until the echo pin is HIGH (that marks the beginning of the pulse length we want to measure)
			while (_controller.Read(_echo) == PinValue.Low)
			{
				if (DateTime.UtcNow.Ticks - hangTicks > 0)
				{
					return null;
				}
			}

			_timer.Start();

			// Wait until the pin is LOW again, (that marks the end of the pulse we are measuring)
			while (_controller.Read(_echo) == PinValue.High)
			{
				if (DateTime.UtcNow.Ticks - hangTicks > 0)
				{
					return null;
				}
			}

			_timer.Stop();

			// distance = (time / 2) × velocity of sound (34300 cm/s)
			var result = Length.FromCentimeters((_timer.Elapsed.TotalMilliseconds / 2.0) * 34.3);

			if (result.Value > 400)
			{
				// result is more than sensor supports
				// something went wrong
				return null;
			}

			File.AppendAllText("hcslog.txt",$"Calculated Distance {result.Centimeters.ToString()}cm {Environment.NewLine}");

			return result;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_controller is not null)
			{
				if (_controller.IsPinOpen(_echo))
				{
					_controller.ClosePin(_echo);
				}

				if (_controller.IsPinOpen(_trigger))
				{
					_controller.ClosePin(_trigger);
				}
			}

			if (_shouldDispose)
			{
				_controller?.Dispose();
				_controller = null!;
			}
		}
	}
}
