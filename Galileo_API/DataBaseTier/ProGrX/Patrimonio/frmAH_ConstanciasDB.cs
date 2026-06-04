using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHConstanciasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        private const string validaCedula = "La cédula es requerida.";

        public FrmAHConstanciasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene los datos iniciales requeridos por frmAH_Constancias para cargar identificación,
        /// nombre del afiliado y el usuario emisor del reporte.
        /// </summary>
        public ErrorDto<FrmAhConstanciasConsultaResponse?> Patrimonio_frmAH_Constancias_Consulta_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse<FrmAhConstanciasConsultaResponse?>(validaCedula);

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<FrmAhConstanciasConsultaResponse?>("El usuario es requerido.");

            try
            {
                var cedulaNormalizada = cedula.Trim();
                var usuarioNormalizado = usuario.Trim();

                var acceso = _mProGrx.fxSys_RA_Consulta(codEmpresa, cedulaNormalizada, usuarioNormalizado);
                if (acceso.Code == -1)
                    return DbHelper.CreateErrorResponse<FrmAhConstanciasConsultaResponse?>(acceso.Description!);

                if (!acceso.Result)
                {
                    return DbHelper.CreateErrorResponse<FrmAhConstanciasConsultaResponse?>(
                        "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!",
                        -2);
                }

                const string sql = @"
select
    rtrim(p.Cedula) as cedula,
    rtrim(p.Nombre) as nombre,
    rtrim(isnull(u.Descripcion, '')) as emitido_por,
    cast('' as varchar(120)) as puesto,
    cast('A quién interese' as varchar(150)) as dirigido_a,
    cast(1 as int) as tipo_constancia,
    cast(0 as bit) as usa_identificacion_alterna
from vPAT_Consolidado p
left join Usuarios u on u.Nombre = @usuario
where p.Cedula = @cedula;";

                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.QueryFirstOrDefault<FrmAhConstanciasConsultaResponse>(
                    sql,
                    new
                    {
                        cedula = cedulaNormalizada,
                        usuario = usuarioNormalizado
                    });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<FrmAhConstanciasConsultaResponse?>(
                        "No se localizó la persona o sus registros de aportes, verifique...",
                        -2);
                }

                return DbHelper.CreateOkResponse<FrmAhConstanciasConsultaResponse?>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmAhConstanciasConsultaResponse?>(ex.Message);
            }
        }
    }

    public class FrmAhConstanciasConsultaResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string emitido_por { get; set; } = string.Empty;
        public string puesto { get; set; } = string.Empty;
        public string dirigido_a { get; set; } = "A quién interese";
        public int tipo_constancia { get; set; } = 1;
        public bool usa_identificacion_alterna { get; set; } = false;
    }
}
