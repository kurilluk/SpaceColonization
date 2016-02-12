using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

namespace SpaceColonization
{
    public class Particle
    {
        //STATIC FIELD
        static public double eatable_distance = 1; //HACK multipy D
        static public double infuence_distance = 5;
        static public double step_size = 1; //D as distance
        static public List<Particle> Temp = new List<Particle>(); //HACK properties get only
        static public Dictionary<string, List<Particle>> Zones = new Dictionary<string, List<Particle>>();

        //STATIC METHODS
        static public void Create(Point3d position)
        {
            /// GET KEY-ZONE. IF EXIST GET AND ADD,
            /// ELSE CREATE NEW AND ADD PARTICEL TO LIST THEN ADD LIST TO ZONES/SPACE/FIELD;
            
            AddParticle(new Particle(position),true); //FIXME: remove duplicates to add same particle twice
        }

        static private void AddParticle(Particle p)
        { AddParticle(p, false); }

        static private void AddParticle(Particle p, bool ceckDuplicates)
        {
            //if (!Zones.ContainsKey(p.Zone))
            //    Zones.Add(p.Zone, new List<Particle>());
            //Zones[p.Zone].Add(p);
            List<Particle> value;
            if (!Zones.TryGetValue(p.Zone, out value))
            {
                value = new List<Particle>();
                Zones.Add(p.Zone, value);
            }
            if (ceckDuplicates)
                if(value.Contains(p))
                    return;
            value.Add(p);
        }

        static public List<string> GetZones(Point3d pt)
        {
            int x = (int)(Math.Floor(pt.X / Particle.infuence_distance));
            int y = (int)(Math.Floor(pt.Y / Particle.infuence_distance));
            int z = (int)(Math.Floor(pt.Z / Particle.infuence_distance));

            List<string> influence_zones = new List<string>(9);
            influence_zones.Add((x + 1) + "" + (y - 1) + "" + (z + 1));
            influence_zones.Add((x + 1) + "" + y + "" + (z + 1));
            influence_zones.Add((x + 1) + "" + (y + 1) + "" + (z + 1));
            influence_zones.Add(x + "" + (y - 1) + "" + (z + 1));
            influence_zones.Add(x + "" + y + "" + (z + 1));
            influence_zones.Add(x + "" + (y + 1) + "" + (z + 1));
            influence_zones.Add((x - 1) + "" + (y - 1) + "" + (z + 1));
            influence_zones.Add((x - 1) + "" + y + "" + (z + 1));
            influence_zones.Add((x - 1) + "" + (y + 1) + "" + (z + 1));

            influence_zones.Add((x+1) + "" + (y-1) + "" + z);
            influence_zones.Add((x+1) + "" + y + "" + z);
            influence_zones.Add((x+1) + "" + (y+1) + "" + z);
            influence_zones.Add(x + "" + (y-1) + "" + z);
            influence_zones.Add(x + "" + y + "" + z);
            influence_zones.Add(x + "" + (y+1) + "" + z);
            influence_zones.Add((x-1) + "" + (y-1) + "" + z);
            influence_zones.Add((x-1) + "" + y + "" + z);
            influence_zones.Add((x-1) + "" + (y+1) + "" + z);

            influence_zones.Add((x + 1) + "" + (y - 1) + "" + (z - 1));
            influence_zones.Add((x + 1) + "" + y + "" + (z - 1));
            influence_zones.Add((x + 1) + "" + (y + 1) + "" + (z - 1));
            influence_zones.Add(x + "" + (y - 1) + "" + (z - 1));
            influence_zones.Add(x + "" + y + "" + (z - 1));
            influence_zones.Add(x + "" + (y + 1) + "" + (z - 1));
            influence_zones.Add((x - 1) + "" + (y - 1) + "" + (z - 1));
            influence_zones.Add((x - 1) + "" + y + "" + (z - 1));
            influence_zones.Add((x - 1) + "" + (y + 1) + "" + (z - 1));

            return influence_zones;
        }

        static public bool IsEatable(Point3d food)
        {
            //find closest particle to food
            double closest_distance = double.MaxValue;
            Particle p_closest = null;

            /// GET KEY-ZONES FOR FOOD (0/+/- X,Y,Z)
            /// FOR EACH ZONES - IF EXISTS, FIND CLOSEST PARTICLE

            List<string> zones = GetZones(food);
            foreach (string zone_id in zones)
            {
                List<Particle> zone;
                if (Zones.TryGetValue(zone_id, out zone))
                {
                    for (int i = 0; i < zone.Count(); i++)
                    {
                        double actual_distance = food.DistanceTo(zone[i].Position);
                        if (actual_distance < closest_distance)
                        {
                            p_closest = zone[i];
                            closest_distance = actual_distance;
                            //if (closest_distance < infuence_distance * 0.2)
                                //continue;
                        }
                    }
                }
            }


            //===: closest particle is particle with index x in the field
            /// STORE AS A LINK TO THE PARTICLE - THEN TARGET FOOD

            //if food is too close - eat it, otherwise target it
            if (closest_distance <= eatable_distance)
                return true;
            else
            {
                if (closest_distance <= infuence_distance)
                {
                    p_closest.TargetFood(food, infuence_distance - closest_distance); //(infuence_distance - closest_distance) as factor
                    Temp.Add(p_closest);
                }
                //prepare to move
                return false;
            }
        }

        static public void Move()
        {

            for (int i = 0; i < Temp.Count; i++)
            {
                Particle clone = Temp[i].Clone();
                if (clone != null)
                    AddParticle(clone);
            }

            /// !!! FOR EACH PARTICLES - UFF OR ONLY LAST ADDED? - LIST TO MOVE OR DIRECTLY MOVE???
            //List<Particle> temp_field = new List<Particle>();
            //for (int p = 0; p < Field.Count(); p++)
            //{
            //    Particle clone = Field[p].Clone();
            //    if (clone != null)
            //        temp_field.Add(clone);
            //}
            //Field.AddRange(temp_field);
        }

        //static public List<Point3d> Positions()
        //{
        //    List<Point3d> positions = new List<Point3d>(Field.Count());
        //    for (int p = 0; p < Field.Count(); p++)
        //    {
        //        positions.Add(Field[p].Position);
        //    }
        //    return positions;
        //}

        static public List<Line> Connections()
        {
            List<Line> connections = new List<Line>();
            foreach (KeyValuePair<string, List<Particle>> pair in Zones)
            {
                for (int p = 0; p < pair.Value.Count(); p++)
                {
                    if (pair.Value[p].Connection != Line.Unset)
                        connections.Add(pair.Value[p].Connection);
                }
            }
            return connections;
        }


        //***************************************************************************

        //FIELD
        public Point3d Position { get; private set; }
        public Line Connection { get; private set; }
        private Vector3d direction;


        public string Zone { get; private set; }

        //CONSTRUCTOR
        Particle(Point3d position)
        {
            this.Position = position;
            this.direction = Vector3d.Zero;
            this.Connection = Line.Unset;
            this.Zone = this.GetZone(this.Position);
        }

        Particle(Point3d position, Vector3d vector)
        {
            this.Position = position + vector;
            this.direction = Vector3d.Zero;
            this.Connection = new Line(this.Position, position);
            this.Zone = this.GetZone(this.Position);
        }

        //METHODS
        private string GetZone(Point3d pt)
        {
            int x = (int)(Math.Floor(pt.X / Particle.infuence_distance));
            int y = (int)(Math.Floor(pt.Y / Particle.infuence_distance));
            int z = (int)(Math.Floor(pt.Z / Particle.infuence_distance));
            return x + "" + y + "" + z; //FIXME: 3 17 -6 a 31 7 -6!
        }

        void TargetFood(Point3d food, double factor)
        {
            Vector3d food_direction = food - this.Position;
            food_direction.Unitize();
            food_direction *= factor; //add factor as double
            this.direction += food_direction;
            //this.direction.Unitize();
        }

        Particle Clone()
        {
            if (this.direction != Vector3d.Zero)
            {
                this.direction.Unitize();
                Particle clone = new Particle(this.Position, (this.direction * step_size));
                this.direction = Vector3d.Zero;
                return clone;
            }
            else
                return null;
        }
    }
}
