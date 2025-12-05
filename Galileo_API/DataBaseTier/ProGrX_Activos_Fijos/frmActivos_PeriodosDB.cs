using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;


namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosPeriodosDB
    {
        private readonly PortalDB _portalDB;
        public FrmActivosPeriodosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para consultar los periodos por estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>

        public ErrorDto<ActivosPeriodosDataLista> Activos_Periodos_Consultar(int CodEmpresa, string estado)
        {
            var meses = new Dictionary<int, string>
                {
                    {1, "ENERO"}, {2, "FEBRERO"}, {3, "MARZO"}, {4, "ABRIL"},
                    {5, "MAYO"}, {6, "JUNIO"}, {7, "JULIO"}, {8, "AGOSTO"},
                    {9, "SEPTIEMBRE"}, {10, "OCTUBRE"}, {11, "NOVIEMBRE"}, {12, "DICIEMBRE"}
                };

            var result = new ErrorDto<ActivosPeriodosDataLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPeriodosDataLista()
                {
                    total = 0,
                    lista = new List<ActivosFijosPeriodosData>()
                }
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select ANIO,mes, dbo.fxActivos_FechaAnioMesToDatetime(Anio,Mes) as 'PeriodoCorte'
                                    from Activos_Periodos where estado = @estado order by anio,mes";
                var _lista = connection.Query<ActivosFijosPeriodosData>(query, new { estado }).ToList();


                _lista.ForEach(item =>
                {
                    if (meses.ContainsKey(item.mes))
                    {
                        item.periodo = $"{meses[item.mes]} DE {item.anio}";
                    }
                });

                result.Result.lista = _lista;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}