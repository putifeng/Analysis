namespace Test ;

    public class TestNewFree
    {

        public Plane plane;
        
        public Plane plane3 { get; set; }
        public Plane plane2;
        public Dictionary<string,Plane> planeMaps = new  Dictionary<string,Plane>();

        
        public TestNewFree()
        {
            Init();
        }

        public void Init()
        {   
            plane = new ();
            
            plane = new Plane();

            plane2 = new Plane();

            
            Dictionary<string,Plane> planeMaps2 = new  Dictionary<string,Plane>();
            
            // var localPlane = new Plane();
            if (!planeMaps.TryGetValue("", out var localPlane))
            {
                localPlane = new Plane();
            }
            
            
            if (!planeMaps2.TryGetValue("", out var localPlane2))
            {
                localPlane2 = new Plane();
            }
            
            
            if (!planeMaps2.TryGetValue("", out plane2))
            {
                plane2 = new Plane();
            }
            
        }

        void Free()
        {
            
            plane.Free();
            
            plane2?.Free();
            
            // foreach (var keyValuePair in planeMaps)
            // {
            //     keyValuePair.Value.Free();
            // }
            
            // var plane2 = new Plane();
            // plane2?.Free();
        }
        
        
    }

    public class Plane : BaseView
    {

        public void Free()
        {
            
        }
    }

    public class BaseView
    {
        
    }