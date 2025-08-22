# unity_env

## CAD to Unity
- After creating the CAD model, export the CAD model to a .3MT file.
- Then convert the .3MT file to a .fbx file by using the link: https://products.aspose.app/3d/conversion/3mf-to-fbx
- In Unity, go to Assets ->  Import New Asset and then click on the .fbx file
- There may be an error with imported asset: "ImportFBX Warnings: Can't import normals, because mesh".
  - A way to fix it is to click on the import asset, look for the option "normal", click on it, and then change "import" to "calculate"
  - Click on "Apply" at the end.
- Congrats, you imported your CAD model into Unity.
