using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Persistence
{
    public class IMSDataAccess : IIMSDataAccess
    {
        public async Task<IMSData> LoadSimulationAsync(String path)
        {
            List<Pod> podData = new List<Pod>();
            List<Destination> destinationData = new List<Destination>();
            List<Dock> dockData = new List<Dock>();
            List<Robot> robotData = new List<Robot>();
            List<RobotUnderPod> robotUnderPodData = new List<RobotUnderPod>();
            
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    //TODO: test if format is correct. For now we assume it is
                    String line; 
                    line = await reader.ReadLineAsync();
                    String[] numbers;
                    numbers = line.Split(' ');
                    Int32 sizeX = Int32.Parse(numbers[0]);
                    Int32 sizeY = Int32.Parse(numbers[1]);
                    line = await reader.ReadLineAsync();
                    Int32 timeStep = Int32.Parse(line);
                    line = await reader.ReadLineAsync();
                    Int32 numEntities = Int32.Parse(line);
                    EntityType type;
                    Int32 x;
                    Int32 y;
                    Int32 totalEnergyConsumption = 0;
                    for (int i = 0; i < numEntities; ++i)
                    {
                        line = await reader.ReadLineAsync();
                        type = (EntityType)Enum.Parse(typeof(EntityType), line);
                        line = await reader.ReadLineAsync();
                        numbers = line.Split(' ');
                        x = Int32.Parse(numbers[0]);
                        y = Int32.Parse(numbers[1]);
                        Int32 capacity;
                        Int32 energyLeft;
                        Int32 energyConsumption;
                        Int32 destinationID;
                        Int32 productIDCount;
                        Robot robot = null;
                        Pod pod;
                        Int32 productID;
                        Int32 productCount;
                        Direction direction;
                        Dictionary<Int32, Int32> products;

                        if (type == EntityType.Dock)
                        {
                            dockData.Add(new Dock(x,y));
                        }
                        else if (type == EntityType.Destination)
                        {
                            line = await reader.ReadLineAsync();
                            Int32 id = Int32.Parse(line);
                            destinationData.Add(new Destination(x,y,id));
                        }
                        else
                        {
                            if (type == EntityType.Robot || type == EntityType.RobotUnderPod)
                            {
                                //in both cases we need to read the robot info first
                                line = await reader.ReadLineAsync();
                                capacity = Int32.Parse(line);
                                line = await reader.ReadLineAsync();
                                energyLeft = Int32.Parse(line);
                                line = await reader.ReadLineAsync();
                                energyConsumption = Int32.Parse(line);
                                line = await reader.ReadLineAsync();
                                direction = (Direction)Enum.Parse(typeof(Direction), line);
                                line = await reader.ReadLineAsync();
                                destinationID = Int32.Parse(line);
                                totalEnergyConsumption += energyConsumption;

                                robot = new Robot(x, y, direction, capacity, energyLeft, destinationID, energyConsumption);
                            
                                if (type == EntityType.Robot)
                                {
                                    robotData.Add(robot);
                                }
                            }
                            if (type == EntityType.Pod || type == EntityType.RobotUnderPod)
                            {
                                //continuing the second part of robotunderpod info
                                line = await reader.ReadLineAsync();
                                productIDCount = Int32.Parse(line);
                                products = new Dictionary<int, int>();
                                for (int j = 0; j < productIDCount; ++j)
                                {
                                    line = await reader.ReadLineAsync();
                                    numbers = line.Split(' ');
                                    productID = Int32.Parse(numbers[0]);
                                    productCount = Int32.Parse(numbers[1]);
                                    //we assume that productIDs aren't repeated in the input text file
                                    products.Add(productID, productCount);
                                }

                                pod = new Pod(x, y, products);

                                if (type == EntityType.Pod)
                                {
                                    podData.Add(pod);
                                }
                                else if(type == EntityType.RobotUnderPod)
                                {
                                    robotUnderPodData.Add(new RobotUnderPod(x, y, robot, pod));
                                }
                            }

                        }
                    }
                    return new IMSData(podData, destinationData, dockData, robotData, robotUnderPodData, sizeX, sizeY, timeStep, totalEnergyConsumption);
                }
            }
            catch
            {
                throw new IMSDataException("");
            }
        }

        public Task SaveSimulationAsync(String path, IMSData values)
        {
            return null;
        }
    }
}
