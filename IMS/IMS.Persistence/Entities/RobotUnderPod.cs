using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class RobotUnderPod : Entity
    {
        private Int32 _energyConsumption;
        private Int32 _capacity;
        private Int32 _energyLeft;
        private Int32 _destinationID;
        private Dictionary<Int32, Int32> _products;

        public Int32 TotalEnergyUsed { get { return _energyConsumption; } }
        public Int32 Capacity { get { return _capacity; } }
        public Int32 EnergyLeft { get { return _energyLeft; } }
        public Int32 DestinationID { get { return _destinationID; } }

        public RobotUnderPod(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID, int energyConsumption, Dictionary<Int32, Int32> products) : base(x, y)
        {
            _type = EntityType.RobotUnderPod;
            _capacity = capacity;
            _energyLeft = energyLeft;
            _energyConsumption = energyConsumption;
            _dir = direction;
            _products = products;
        }

        public RobotUnderPod(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID, Dictionary<Int32, Int32> products) : this(x, y, direction, capacity, energyLeft, destinationID, 0, products)
        {
        }

        public RobotUnderPod(int x, int y, Robot robot, Pod pod) : this(x, y, robot.Direction, robot.Capacity, robot.EnergyLeft, robot.DestionationID, robot.EnergyConsumption, pod.Products)
        {
        }

        public void RemoveProduct(int productID)
        {
            if (_products.ContainsKey(productID))
            {
                if (_products[productID] == 1)
                {
                    _products.Remove(productID);
                }
                else if (_products[productID] > 1)
                {
                    _products[productID] -= 1;
                }
                else
                {
                    //might need to throw an exception if this is even possible (products[productID] == 0 shouldn't be allowed)
                }
            }
            else
            {
                //might need to throw an exception if someone is trying to remove a nonexistent productID
            }
        }
    }
}
