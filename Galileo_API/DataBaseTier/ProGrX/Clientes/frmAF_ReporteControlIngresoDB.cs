using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfReporteControlIngresoDB
    {
        private readonly IConfiguration _config;

        private const string OpcionTodos = "T";

        private const string SqlEstadosPersona = @"
                    SELECT cod_estado AS item,
                           descripcion
                    FROM dbo.AFI_Estados_Persona
                    ORDER BY cod_estado ASC;";

        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           descripcion
                    FROM dbo.Instituciones
                    ORDER BY cod_institucion ASC;";

        public FrmAfReporteControlIngresoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los estados de persona disponibles para el reporte de control de ingreso.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de estados de persona.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ReporteControlIngresoEstado_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlEstadosPersona);

            if (result.Code == 0)
            {
                InsertarOpcionTodos(result.Result);
            }

            return result;
        }


        /// <summary>
        /// Obtiene las instituciones disponibles para el reporte de control de ingreso.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ReporteControlIngresoInstitucion_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);

            if (result.Code == 0)
            {
                InsertarOpcionTodos(result.Result);
            }

            return result;
        }


        /// <summary>
        /// Inserta la opción TODOS al inicio de la lista.
        /// </summary>
        /// <param name="lista">Lista a modificar.</param>
        private static void InsertarOpcionTodos(List<DropDownListaGenericaModel>? lista)
        {
            lista ??= new List<DropDownListaGenericaModel>();

            lista.Insert(0, new DropDownListaGenericaModel
            {
                item = OpcionTodos,
                descripcion = "TODOS"
            });
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}