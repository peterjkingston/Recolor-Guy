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
  -Pixel targeting updated, but still sometimes grabs pixels of unexpected spectrum.
  More work to be done here.
  
  -Image displayers Top property on MainWindow.xaml not bound to ComboBoxes Bottom 
  property. Binding this will form a more appropriate layout similar to a constraint-
  layout. More work to be done here.
  
  -Expect Out of Memory exceptions to occur when near maximum memory usage, or when using
  very large image files expect an Access Violation exceptions. More work to be done here.
  
Future Improvements:
  -Create a more visually appealing user control for selecting to and from spectrums.
  Ideally, this control should have a conical nature, including a cone face, and side
  view.
