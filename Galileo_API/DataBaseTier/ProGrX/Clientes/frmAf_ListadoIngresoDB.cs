using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfListadoIngresoDB
    {
        private readonly IConfiguration _config;
        
        private const string SqlEstados = @"
                    SELECT cod_estado AS item,
                           descripcion
                    FROM dbo.AFI_Estados_Persona;";

        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           descripcion
                    FROM dbo.instituciones;";

        public FrmAfListadoIngresoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el listado de estados de ingreso.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de estados.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ListadoIngreso_Estados_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlEstados);
        }


        /// <summary>
        /// Obtiene el listado de instituciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ListadoIngreso_Instituciones_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);

            if (result.Code == 0 && result.Result is not null)
            {
                result.Result.Insert(0, new DropDownListaGenericaModel
                {
                    item = "T",
                    descripcion = "Todas"
                });
            }

            return result;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);
    }
}