# Many-World Browsing In Unity (MWB)

## Introduction
This is a Unity native physic-based animation aiding system. Aiming to generate plausible paths for lots of Rigidbodies and still remain the ability to customize your personal animation target.

## How To Use
First Create a physic system by adding a empty parent GameObject and append physic objects as children. 

For any objects that you want to be out of the exported animation, or just want it to be providing a static collider, mark it as static.

Add the "MWB_System" Component to the parent object, it will provide a checkbox list of all child GameObjects that have both collider and rigidbody component. You can check the object in the list to trace its path, we recommand marking objects that isn't traced as static.

After setting up these, enter play mode and press simulate. You will see the system gradually simulating. You can open our "MWB Anim" window to get a rough idea on how long it'll take, but if the simulation goes too long, we recommand cancelling and start again with different parameters.

The parameters in MWB_System are explaned as follow :

FrameCount : the total frame count each world will be simulated.

WorldForkedOnCollision : the count of worlds forked when a valid collision happens.

Physics Setup/Threshold Coef : a coefficient used to determine whether a collision is valid. A valid collision is a collision with impulse magnitude greater than this coefficient multiply by the object's mass( in rigidbody ).

## Credit

This project is developed by Alan Shih and David Yu as an undergraduate independent research in NCTU, Taiwan.