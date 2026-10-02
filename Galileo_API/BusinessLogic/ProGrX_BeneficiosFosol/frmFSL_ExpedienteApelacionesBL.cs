using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslExpedienteApelacionesBl
    {
        private readonly FrmFslExpedienteApelacionesDb _db;

        public FrmFslExpedienteApelacionesBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db =
                new FrmFslExpedienteApelacionesDb(
                    config);
        }

        public ErrorDto<FslExpedienteDatos>
            FSL_ExpedienteApelaciones_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _db
                .FSL_ExpedienteApelaciones_Expediente_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_ExpedienteApelaciones_Catalogo_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_ExpedienteApelaciones_Catalogo_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<FslExpedienteApelacionData>>
            FSL_ExpedienteApelaciones_Historico_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _db
                .FSL_ExpedienteApelaciones_Historico_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        public ErrorDto<
            List<FslExpedienteResolucionMiembroData>>
            FSL_ExpedienteApelaciones_Miembros_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _db
                .FSL_ExpedienteApelaciones_Miembros_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        public ErrorDto<string>
            FSL_ExpedienteApelaciones_UsuarioVinculado_Obtener(
                int CodEmpresa,
                string cedula,
                string codComite)
        {
            return _db
                .FSL_ExpedienteApelaciones_UsuarioVinculado_Obtener(
                    CodEmpresa,
                    cedula,
                    codComite);
        }

        public ErrorDto
            FSL_ExpedienteApelaciones_Miembro_Validar(
                int CodEmpresa,
                FslExpedienteMiembroValidarRequest? request)
        {
            return _db
                .FSL_ExpedienteApelaciones_Miembro_Validar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_ExpedienteApelaciones_Apelacion_Agregar(
                int CodEmpresa,
                FslExpedienteApelacionAgregarRequest? request)
        {
            return _db
                .FSL_ExpedienteApelaciones_Apelacion_Agregar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_ExpedienteApelaciones_Resolucion_Guardar(
                int CodEmpresa,
                FslExpedienteApelacionResolucionGuardarRequest?
                    request)
        {
            return _db
                .FSL_ExpedienteApelaciones_Resolucion_Guardar(
                    CodEmpresa,
                    request);
        }
    }
}