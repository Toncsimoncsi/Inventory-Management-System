using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Model
{
    public interface IIMSModel
    {


        /// <summary>
        /// Mezőérték lekérdezése, vagy beállítása.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <returns>A mező értéke.</returns>
        Entity this[Int32 x, Int32 y] { get; }

        /// <summary>
        ///  Játék kezdetének eseménye.
        /// </summary>
        //event EventHandler SimulationStarted;

        /// <summary>
        /// Játék végének eseménye.
        /// </summary>
        //event EventHandler SimulationOver;

        /// <summary>
        /// Mezőváltozás eseménye.
        /// </summary>
        event EventHandler<FieldChangedEventArgs> FieldChanged;

        /// <summary>
        /// Új játék indítása.
        /// </summary>
        void NewSimulation();

        /// <summary>
        /// Játék betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        Task LoadSimulationAsync(String path);

        /// <summary>
        /// Játék mentése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        Task SaveSimulationAsync(String path);

    }
}
