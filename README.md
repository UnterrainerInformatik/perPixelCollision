# perPixelCollision

This is a repo to coordinatinate our efforts to build a small example of part of a library in order to maybe merge it into MonoGame.Extended later on.

Used the MonoGame.Extended demo project for illustration.

## Contents
* Load textures from XNB.-file.
* Get the color-data from textures for test.
* Transform the color-data into a smaller bool-array containing true for pixels with alpha>0.
* Get the transformation matrix for a texture being rotated around a custom center, scaled and positioned.
* Per pixel collision-testing method for those textures and matrices.
* Color the sprites read when they per-pixel-intersect.
