using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslExpedienteGestionesBL
    {
        private readonly FrmFslExpedienteGestionesDB _db;

        public FrmFslExpedienteGestionesBL(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db =
                new FrmFslExpedienteGestionesDB(
                    config);
        }

        public ErrorDto<FslExpedienteDatos>
            FSL_ExpedienteGestiones_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _db
                .FSL_ExpedienteGestiones_Expediente_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_ExpedienteGestiones_Catalogo_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_ExpedienteGestiones_Catalogo_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<FslExpedienteGestionData>>
            FSL_ExpedienteGestiones_Historico_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _db
                .FSL_ExpedienteGestiones_Historico_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        public ErrorDto
            FSL_ExpedienteGestiones_Gestion_Agregar(
                int CodEmpresa,
                FslExpedienteGestionAgregarRequest?
                    request)
        {
            return _db
                .FSL_ExpedienteGestiones_Gestion_Agregar(
                    CodEmpresa,
                    request);
        }
    }
}