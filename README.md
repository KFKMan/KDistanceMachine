# KDistanceMachine

It's a simple scheme for learning, implementations is not good and most of them not finished yet. <br/>

Just think a project like fabric length calculator when you are using traditional ways like lasers, motors it can be very complex and expensive. <br/>
With this project you can calculate without touch and a lot of sensors. <br/>

This project helps you to calculate the length of the object with distance (height, h) and angle (angle of the camera) <br/>

You can get distance with distance sensor like HCS04 and you can get MaxAngle, MaxPixelCount from documentation of the camera. <br/>

There's two MaxAngle, MaxPixelCount documented which one? If you want to calculate X Length use Horizontal Angle, Horizontal Pixel Count. If you want to calculate Y Lenght use Vertical Angle, Vertical Pixel Count etc. <br/>

<img width="517" alt="prep" src="https://github.com/user-attachments/assets/3aa5c9f8-701f-4230-aa77-0274038151ba">

When we know Height and Target Angle we can just think as 2 triangle and we can calculate each of them. <br/>

For calculating any of them we can use **PartLength = Height/tan(TargetAngle)**, it will give us PartLength we can calculate two PartLength (Right PartLenght and Left PartLenght) and summing them we can get object length. <br/>

Prove; <br/>
tan(TargetAngle) = Height/PartLength <br/>
PartLength = Height/tan(TargetAngle) <br/>

Summing; <br/>
ObjectLength = LeftPartLength + RightPartLength <br/>

But how we can get TargetAngle? <br/>
We can get TargetAngle with MaxAngle, MaxPixelCount and ObjectPixelCount. <br/>
PixelPerAngle = MaxPixelCount/MaxAngle <br/>
TargetAngle = ObjectPixelCount/PixelPerAngle <br/>









