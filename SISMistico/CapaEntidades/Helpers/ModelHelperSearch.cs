using System;
using System.Collections.Generic;

namespace CapaEntidades.Helpers
{
    public class ModelHelperSearch
    {
        public string TipoHelperSearch { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateFinish { get; set; }
        public TimeSpan HourStart { get; set; }
        public TimeSpan HourFinish { get; set; }
        public string TipoBusqueda { get; set; }
        public List<ParameterSQLModel> ParametrosBusqueda { get; set; }
    }

    public class ParameterSQLModel
    {
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }
    }
}
