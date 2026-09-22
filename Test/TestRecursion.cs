namespace Test ;

    public class TestRecursion : IPropertyTest
    {
        // public string testStr => testStr;

        public SubTest subTest;
        public string testStr2
        {
            get => testStr2;
            set => testStr2 = value;
        }
        
        public string testStr4
        {
            get => subTest.testStr4;
            set => subTest.testStr4 = value;
        }
        public string value
        {
            get => subTest.value;
            set => subTest.value = value;
        }

        public string testProperty3
        {
            get
            {
                string testProperty3 = "";
                return testProperty3;
            }
            set
            {
                testProperty3 = value;
            } 
        }
        public string TestPropretyStr { get; }
        string IPropertyTest.TestPropretyStr => TestPropretyStr;

        public class SubTest
        {
            public string testStr4 { get; set; }
            public string value { get; set; }
        }
    }

    public interface IPropertyTest
    {
        public string TestPropretyStr { get; }
    }