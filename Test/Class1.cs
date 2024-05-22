// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global


// If you don't see warnings, build the Analyzers Project.

using System;

public class Examples
{
    public class MyCompanyClass // Try to apply quick fix using the IDE.
    {
    }
    [TestArrribute("中文")]
    string abs3 = "中文";
    string abs2 = LanguageCode.abs2;
    public void ToStars()
    {
        var spaceship = new Spaceship();
        spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        string abc = abs2;
        spaceship.SetSpeed(42);

        TTT(300000000);
        log(abc == "abc" ? $"中文{10}x":"");
        TTT($"中文{10}x");
        TTT("中文");
        log("中文");
    }
    
    private void log(string a){}
    private void TTT(string a){}
    private void TTT(int a)
    {
        
    }
    
    class TestArrribute : Attribute
    {
        public TestArrribute(string a)
        {

        }
    }
    
    [TestArrribute("中文")]
    public class Spaceship
    {
        public void SetSpeed(long speed)
        {
            if (speed > 299_792_458)
                throw new Exception(nameof(speed));
        }
    }
}