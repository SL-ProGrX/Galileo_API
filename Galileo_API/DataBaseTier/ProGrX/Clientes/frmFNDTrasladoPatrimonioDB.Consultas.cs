using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmFndTrasladoPatrimonioDB
    {
        public ErrorDto<List<FndTrasladoPatrimonioPlan>> Fnd_TrasladoPatrimonio_Planes_Obtener(
            int CodEmpresa,
            string IdOperadora)
        {
            return DbHelper.ExecuteListQuery<FndTrasladoPatrimonioPlan>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanesPatrimonio,
                new { IdOperadora = NormalizarTexto(IdOperadora) });
        }

        public ErrorDto<FndTrasladoPatrimonioDetalle?> Fnd_TrasladoPatrimonio_PlanDetalle_Obtener(
            int CodEmpresa,
            string IdOperadora,
            string CodPlan)
        {
            return DbHelper.ExecuteSingleQuery<FndTrasladoPatrimonioDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanDetalle,
                null,
                new
                {
                    IdOperadora = NormalizarTexto(IdOperadora),
                    CodPlan = NormalizarTexto(CodPlan)
                });
        }

        public ErrorDto<List<FndTrasladoPatrimonioContrato>> Fnd_TrasladoPatrimonio_Contratos_Obtener(
            int CodEmpresa,
            string IdOperadora,
            string CodPlan,
            string Destino,
            bool Marcado)
        {
            return DbHelper.ExecuteListQuery<FndTrasladoPatrimonioContrato>(
                CreatePortalDb(),
                CodEmpresa,
                SqlContratosPatrimonio,
                CrearParametrosContratos(IdOperadora, CodPlan, Destino, Marcado));
        }

        public ErrorDto<List<FndTrasladoPatrimonioSocioDetalle>> Fnd_TrasladoPatrimonio_SocioDetalle_Obtener(
            int CodEmpresa,
            string Tcon,
            string Ncon)
        {
            return DbHelper.ExecuteListQuery<FndTrasladoPatrimonioSocioDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                SqlSocioDetalle,
                new
                {
                    Tcon = NormalizarTexto(Tcon),
                    Ncon = NormalizarTexto(Ncon)
                });
        }

        public ErrorDto<List<FndAhorroDetalladoResumen>> Fnd_AhorroDetallado_Resumen_Obtener(
            int CodEmpresa,
            FndAhorroDetalladoResumenRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del resumen son requeridos.",
                    -2,
                    new List<FndAhorroDetalladoResumen>());
            }

            return DbHelper.ExecuteListQuery<FndAhorroDetalladoResumen>(
                CreatePortalDb(),
                CodEmpresa,
                SqlAhorroDetalladoResumen,
                new
                {
                    TipoDoc = NormalizarTexto(request.TipoDoc),
                    NC_Pat = NormalizarTexto(request.NC_Pat)
                });
        }

        public ErrorDto<ParAfahCuentasResult?> ParAfah_Cuentas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteSingleQuery<ParAfahCuentasResult>(
                CreatePortalDb(),
                CodEmpresa,
                SqlParAfahCuentas,
                null);
        }

        public ErrorDto<FndTrasladoPatrimonioGlobalesResult?> Fnd_TrasladoPatrimonio_Globales_Obtener(
            int CodEmpresa,
            string usuario,
            int codContabilidad)
        {
            var globales = _mProGrx
                .sbSifParametrosInicializa(CodEmpresa, usuario, codContabilidad)
                .Result;

            return DbHelper.CreateOkResponse<FndTrasladoPatrimonioGlobalesResult?>(
                new FndTrasladoPatrimonioGlobalesResult
                {
                    OficinaTitular = globales.GOficinaTitular ?? string.Empty,
                    OficinaUnidad = globales.GOficinaUnidad ?? string.Empty,
                });
        }
    }
}