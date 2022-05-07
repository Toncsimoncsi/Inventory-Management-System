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
                            dockData.Add(new Dock(x, y));
                        }
                        else if (type == EntityType.Destination)
                        {
                            line = await reader.ReadLineAsync();
                            Int32 id = Int32.Parse(line);
                            destinationData.Add(new Destination(x, y, id));
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
                                else if (type == EntityType.RobotUnderPod)
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

        public async Task SaveSimulationAsync(String path, IMSData values)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    await writer.WriteLineAsync(values.SizeX.ToString() + " " + values.SizeY.ToString());
                    await writer.WriteLineAsync(values.Time.ToString());
                    await writer.WriteLineAsync((values.EntityData.PodData.Count + values.EntityData.RobotData.Count + values.EntityData.RobotUnderPodData.Count + values.EntityData.DestinationData.Count + values.EntityData.DockData.Count).ToString());
                    foreach (Pod entity in values.EntityData.PodData)
                    {
                        await writer.WriteLineAsync("Pod");
                        await writer.WriteLineAsync(entity.Pos.X.ToString() + " " + entity.Pos.Y.ToString());
                        await writer.WriteLineAsync(entity.Products.Count.ToString());
                        foreach (Int32 product in entity.Products.Keys)
                        {
                            await writer.WriteLineAsync(product.ToString() + " " + entity.Products[product]);
                        }
                    }
                    foreach (Robot entity in values.EntityData.RobotData)
                    {
                        await writer.WriteLineAsync("Robot");
                        await writer.WriteLineAsync(entity.Pos.X.ToString() + " " + entity.Pos.Y.ToString());
                        await writer.WriteLineAsync(entity.Capacity.ToString());
                        await writer.WriteLineAsync(entity.EnergyLeft.ToString());
                        await writer.WriteLineAsync(entity.EnergyConsumption.ToString());
                        await writer.WriteLineAsync(entity.Direction.ToString());
                        await writer.WriteLineAsync(entity.DestinationID.ToString());
                    }
                    foreach (RobotUnderPod entity in values.EntityData.RobotUnderPodData)
                    {
                        await writer.WriteLineAsync("RobotUnderPod");
                        await writer.WriteLineAsync(entity.Pos.X.ToString() + " " + entity.Pos.Y.ToString());
                        await writer.WriteLineAsync(entity.Capacity.ToString());
                        await writer.WriteLineAsync(entity.EnergyLeft.ToString());
                        await writer.WriteLineAsync(entity.EnergyConsumption.ToString());
                        await writer.WriteLineAsync(entity.Direction.ToString());
                        await writer.WriteLineAsync(entity.DestinationID.ToString());

                        await writer.WriteLineAsync(entity.Products.Count.ToString());
                        foreach (Int32 product in entity.Products.Keys)
                        {
                            await writer.WriteLineAsync(product.ToString() + " " + entity.Products[product]);
                        }
                    }
                    foreach (Dock entity in values.EntityData.DockData)
                    {
                        await writer.WriteLineAsync("Dock");
                        await writer.WriteLineAsync(entity.Pos.X.ToString() + " " + entity.Pos.Y.ToString());
                    }
                    foreach (Destination entity in values.EntityData.DestinationData)
                    {
                        await writer.WriteLineAsync("Destination");
                        await writer.WriteLineAsync(entity.Pos.X.ToString() + " " + entity.Pos.Y.ToString());
                        await writer.WriteLineAsync(entity.ID.ToString());
                    }
                }
            }
            catch
            {
                throw new IMSDataException("");
            }
        }

        public async Task SaveDiaryAsync(String path, IMSData values)
        {
            //steps
            //per robot energy consumption
            //total energy consumption
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    await writer.WriteLineAsync("Total steps required: " + values.StepCount.ToString());
                    int i = 0;
                    int totalEnergy = 0;
                    await writer.WriteLineAsync("Energy consumed by each robot:");
                    foreach (Robot robot in values.EntityData.RobotData){
                        ++i;
                        totalEnergy += robot.EnergyConsumption;
                        await writer.WriteLineAsync("Robot " + i.ToString() + ": " + robot.EnergyConsumption.ToString());
                    }
                    await writer.WriteLineAsync("Total energy consumption: " + totalEnergy.ToString());
                }
            }
            catch
            {
                throw new IMSDataException("");
            }

        }

        public int getTotalEnergy(IMSData values)
        {
            int i = 0;
            int totalEnergy = 0;
            foreach (Robot robot in values.EntityData.RobotData)
            {
                ++i;
                totalEnergy += robot.EnergyConsumption;
            }
            return totalEnergy;
        }
    }

}
