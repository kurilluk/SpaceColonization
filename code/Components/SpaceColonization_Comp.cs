using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SpaceColonization
{
    public class SpaceColonization_Comp : GH_Component
    {

        public SpaceColonization_Comp()
            : base("SpaceColonization", "SC", "Colonize space", "FARM", "Other")
        { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Food", "food", "food", GH_ParamAccess.list); //0
            pManager[0].Optional = true;
            pManager.AddPointParameter("Origin", "origin", "origin", GH_ParamAccess.list); //1
            pManager.AddNumberParameter("Step", "D", "Step distance", GH_ParamAccess.item,45); //2
            pManager.AddNumberParameter("Infuence", "di", "Infuence distance", GH_ParamAccess.item,3); //3
            pManager.AddNumberParameter("Kill-Eat", "dk", "Killing distance", GH_ParamAccess.item,1.2); //4
            pManager.AddIntegerParameter("Iteraction", "loop", "Number of iteraction", GH_ParamAccess.item,1); //5
            pManager.AddBooleanParameter("Reset", "reset", "Reset structure", GH_ParamAccess.item,false); //6
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Connections", "C", "Connections", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;
            DA.GetData(6, ref reset);
            if (reset)
                Particle.Zones.Clear(); //= new List<Particle>();

            List<Point3d> origin = new List<Point3d>();
            DA.GetDataList<Point3d>(1, origin);
            foreach (Point3d pt in origin) //FIXME: remove duplicates for each component calling
                Particle.Create(pt);

            double D = 25;
            DA.GetData<double>(2, ref D);
            Particle.step_size = D;

            double dk = 1.2;
            DA.GetData<double>(4, ref dk);
            Particle.eatable_distance = D * dk;

            double di = 4;
            DA.GetData<double>(3, ref di);
            Particle.infuence_distance = D * di;

            int loop = 1;
            DA.GetData<int>(5, ref loop);

            List<Point3d> food = new List<Point3d>();
            bool haveFood = (DA.GetDataList<Point3d>(0, food));

            if (haveFood)
            {
                while (loop > 0)
                {
                    for (int i = food.Count() - 1; i >= 0; i--)
                    {
                        // if food is eatable
                        if (Particle.IsEatable(food[i]))
                        {
                            food.RemoveAt(i); // eat it
                            //Print("food at index " + i + " was removed");
                        }
                    }
                    Particle.Move();
                    loop--;
                }
            }

            DA.SetDataList(0, Particle.Connections());

        }

        public override Guid ComponentGuid
        {
            get { return new Guid("92EC203E-BFFA-4B78-8F57-409812FB0BDB"); }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Space_Colonization.Properties.Resources.SpaceColonization_icon; }
        }
    }
}
