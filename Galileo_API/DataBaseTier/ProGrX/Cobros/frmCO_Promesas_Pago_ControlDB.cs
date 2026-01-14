using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOPromesasPagoControlDB
    {
        private readonly PortalDB _portalDb;

        public FrmCOPromesasPagoControlDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PromesasPago_Usuarios_Obtener(int codEmpresa)
        {
            var query = @"SELECT Usuario AS item, Nombre AS descripcion
                          FROM Cbr_usuarios
                          WHERE estado = 1
                          ORDER BY Nombre";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<PromesasPagoConsultaResult>> PromesasPago_Consulta(PromesasPagoConsultaParams param)
        {
            var query = "exec spCbr_Promesas_Pago_Consulta @PUSUARIO, @PFINICIO, @PFCORTE, @TXTFILTRO";
            var parameters = new
            {
                PUSUARIO = param.Usuario ?? "",
                PFINICIO = param.FInicio,
                PFCORTE = param.FCorte,
                TXTFILTRO = param.Filtro ?? ""
            };
            return DbHelper.ExecuteListQuery<PromesasPagoConsultaResult>(_portalDb, param.CodEmpresa ?? 0, query, parameters);
        }
    }
}
