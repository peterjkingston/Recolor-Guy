# Recolor-Guy
A basic software to target pixels in an image based on hue, and change the color spectrum 
to a relative equivilant hue.

Requires C# v7.2 or higher
Requires NuGet package System.Memory

Sections omitted:
  -Properties & all subfiles
  -References
  -App.xaml & App.cs
  
Known Issues:
  -Program assumes BigEndian, and 32bit processing, detection to be added for added portability.
   More work to be done here.

 -Image aspect ratios do not lock. More work to be done here.

  -Pixel targeting updated, but still sometimes grabs pixels of unexpected spectrum.
   Noticed int RecolorPixel.GetHue(int) is not written in accordance with the reference cited.
   Also, RBG values are getting truncated due to missing cast. More work to be done here.
  
  -Image length still not calculated correctly for all images. More work to be done here.
  
  -Expect Out of Memory exceptions to occur when near maximum memory usage, or when using
  very large image files expect an Access Violation exceptions. More work to be done here.
  
Future Improvements:
  -Create a more visually appealing user control for selecting to and from spectrums.
  Ideally, this control should have a conical nature, including a cone face, and side
  view.
