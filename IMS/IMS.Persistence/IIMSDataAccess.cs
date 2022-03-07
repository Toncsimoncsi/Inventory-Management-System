using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Persistence
{
    public interface IIMSDataAccess
    {
        /// <summary>
        /// Fájl betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A beolvasott mezőértékek.</returns>
        Task<Entity[]> LoadSimulationAsync(String path);

        /// <summary>
        /// Fájl mentése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <param name="values">A mezőértékek.</param>
        Task SaveSimulationAsync(String path, Entity[] values);
    }
}
