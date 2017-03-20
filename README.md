# InputWrapper
A Hare-brained, half-baked, seat-of-your-pants attempt at making a plugabble Input Detection wrapper using the Managed Extensibility Framework (MEF).

[JoystickWrapper](https://autohotkey.com/boards/viewtopic.php?t=28889) surprised me in how easy it was (With a little help) to cobble together an input wrapper in C#, so I decided to try something a little more fancy...  

A DLL that you can load which can load *other* dll plugins - ie an extensible version of JoystickWrapper.  
I found this [awesome article on CodePlex](https://www.codeproject.com/Articles/376033/From-Zero-to-Proficient-with-MEF), do I totally stole Example 04 (Advanced version), and a quick Visual Studio refactor later, InputWrapper was born.  
