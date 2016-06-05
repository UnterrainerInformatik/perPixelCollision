# perPixelCollision

This is a repo to coordinatinate our efforts to build a small example of part of a library in order to maybe merge it into MonoGame.Extended later on.

Used the MonoGame.Extended demo project for illustration.
#### Update
*This is not going to MonoGame.Extended since it's really not useful. Pixel-Perfect collision is too expensive in respect to CPU time, CPU/GPU data transfers or RAM, depending on the implementation.*

# How does it work?
First of all you need an array containing a boolean value of every pixel for every texture you want to check.
We set it to true, if it has an alpha-value greater than zero and false otherwise.
You'll have to do this for every single texture you want to check. So if you have a sprite-atlas and do animations from there, you'd have to do this for every 'frame'.

>So why do we have to do that if we have the textures ready and loaded anyhow? We're drawing it, right? It has to be at least in GPU RAM!  
  
>The reason for that is because your textures reside in the GPU memory and not in CPU memory and it takes forever, in gaming-terms, to transfer data from the GPU back to the CPU (you'd have stalls resulting in a very low framerate).
That's the reason why games don't modify the textures themselves to display decals like bullet holes or tracks in the snow. The process of loading the old texture into CPU RAM takes like forever.
Graphic cards are not built for that.
So it's no viable solution to just 'ask' the texture itself every frame. Instead you'll have to 'ask' it once, on startup, and memorize that data.

## WARNING!
So, that is the reason why it's generally considered a bad idea. Every single point of your textures will take up a boolean value and, if that wasn't bad enough, there is no easy way to do that with just a single bit. Even a boolean value will use a full byte in memory. So it's a giant waste of memory in the best case.

## OK. Now we have the alpha-data. What now?
Now we check for a collision on every frame using our update-loop.
For that we have to take into account the position (translation) and rotation of every texture involved.
The first thing we do is we build a transformation-matrix for each of the two textures.
We are in 3D, so our matrices will have a size of 4x4. Thankfully there are methods ready to be used.

```csharp
    matrix = Matrix.Identity;
// First we translate the texture to the zero-point of our coordinate system, so that we may...
    matrix *= Matrix.CreateTranslation(-origin.X, -origin.Y, 0f);
// scale it from there and most importantly...
    matrix *= Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1f));
// rotate it around that origin-point instead of the top left corner
// (that would usually be the origin)...
    matrix *= Matrix.CreateRotationZ(rotation);
// Then we translate it back, so that it behaves like a normal texture again.
    matrix *= Matrix.CreateTranslation(position.X, position.Y, 0);
```
Think of it this way:  
First we translate our texture to it's origin-point. There we scale and rotate it. Then we translate back to zero.
We now did a roation around a custom center-point and scaling and we did that by multiplying all the matrices in the right order.
Every point we now throw at that matrix will 'do' all those steps and come out with the right coordinates.

When we've done that for the second texture as well, then there is another cool thing about transformation matrices:
There is such a thing as an `inverse matrix`.
It lets you do all the things above in reverse order as well.

```csharp
// Calculate a matrix which transforms from A's local space
// into world space and then into B's local space.
    Matrix bInverted = Matrix.Invert(transB);
    Matrix transformAtoB = transA*bInverted;
```
Now we can check every point in A against every corresponding point in B no matter how scaled, translated or rotated those two are.
You may add other translations yourself. You just have to add them in the method above (the one we used to get the translation matrix for a texture). Just be sure that you add it at the right position.
The nice thing is: No matter how many translations you add, it doesn't impair the calculation of the inverse matrix at all.

## WARNING!
Per pixel collision testing is very expensive since it, in the worst case, compares all points of the first texture to the corresponding points in the second one.

So it's an extremely bad choice when you:
* are doing many tests
* have big textures
* have textures with large amounts of transparent pixels
* want your program to run on any machine other than your desktop (portability, especialy to mobiles)

There are already a few tweaks in the code, like switching the two textures so you only have to iterate over the full set of pixels of the smaller texture, for example.
This spares us many calculation steps since, in the worst case, we don't have a collision.
Then all of the points have to be checked and it's less time consuming to check all points of the smaller texture against the corresponding ones in the bigger one than vice versa.

# Setup
Since this is a MonoGame.Extended demo project it needs MG.Extended obviously.
Open the solution file in VS 2015.
Then open the NuGet console by selecting `Tools` from the menu and then `NuGet Package Manager` and `PackageManager Console` and type the following command:
```
Install-Package MonoGame.Extended
```
Then re-open the project in VS 2015 and run it.
*(You may have to correct the references to MonoGame.Framework.dll or MonoGame.Extended.dll manually. They are located in the packages-subfolder generated by NuGet after the execution of the command above.)*

## Contents
* Load textures from XNB.-file.
* Get the color-data from textures for test.
* Transform the color-data into a smaller bool-array containing true for pixels with alpha>0.
* Get the transformation matrix for a texture being rotated around a custom center, scaled and positioned.
* Per pixel collision-testing method for those textures and matrices.
* Color the sprites read when they per-pixel-intersect.

## Improvements
* Don't re-calculate the transformation-matrix on every update. Cache it and only re-calculate it if any of the parameters change.