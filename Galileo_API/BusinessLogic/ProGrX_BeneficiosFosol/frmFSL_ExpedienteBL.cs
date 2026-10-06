using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslExpedienteBL
    {
        private readonly FrmFslExpedienteDB _db;

        public FrmFslExpedienteBL(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmFslExpedienteDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Planes_Obtener(int CodEmpresa)
            => _db.FSL_Expediente_Planes_Obtener(CodEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Comites_Obtener(int CodEmpresa)
            => _db.FSL_Expediente_Comites_Obtener(CodEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Enfermedades_Obtener(int CodEmpresa)
            => _db.FSL_Expediente_Enfermedades_Obtener(CodEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Causas_Obtener(
                int CodEmpresa,
                string codPlan)
            => _db.FSL_Expediente_Causas_Obtener(
                CodEmpresa,
                codPlan);

        public ErrorDto<FslExpedienteDatos>
            FSL_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<long>
            FSL_Expediente_Navegacion_Obtener(
                int CodEmpresa,
                long codExpediente,
                bool siguiente)
            => _db.FSL_Expediente_Navegacion_Obtener(
                CodEmpresa,
                codExpediente,
                siguiente);

        public ErrorDto<List<FslExpedienteRequisitoData>>
            FSL_Expediente_Requisitos_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_Requisitos_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<List<FslExpedienteOperacionData>>
            FSL_Expediente_Operaciones_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_Operaciones_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<List<FslExpedienteResolucionMiembroData>>
            FSL_Expediente_ResolucionMiembros_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_ResolucionMiembros_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<FslExpedienteResolucionValidacionesData?>
            FSL_Expediente_ResolucionValidaciones_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_ResolucionValidaciones_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<List<FslExpedienteGestionData>>
            FSL_Expediente_Gestiones_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_Gestiones_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<List<FslExpedienteApelacionData>>
            FSL_Expediente_Apelaciones_Obtener(
                int CodEmpresa,
                long codExpediente)
            => _db.FSL_Expediente_Apelaciones_Obtener(
                CodEmpresa,
                codExpediente);

        public ErrorDto<string?>
            FSL_Expediente_UsuarioVinculado_Obtener(
                int CodEmpresa,
                string cedula,
                string codComite)
            => _db.FSL_Expediente_UsuarioVinculado_Obtener(
                CodEmpresa,
                cedula,
                codComite);

        public ErrorDto
            FSL_Expediente_Registro_Validar(
                int CodEmpresa,
                string cedula,
                string codPlan,
                string codCausa)
            => _db.FSL_Expediente_Registro_Validar(
                CodEmpresa,
                cedula,
                codPlan,
                codCausa);

        public ErrorDto<FslExpedienteGuardarResultado>
            FSL_Expediente_Insertar(
                int CodEmpresa,
                FslExpedienteGuardarRequest request)
            => _db.FSL_Expediente_Insertar(
                CodEmpresa,
                request);

        public ErrorDto
            FSL_Expediente_Actualizar(
                int CodEmpresa,
                FslExpedienteGuardarRequest request)
            => _db.FSL_Expediente_Actualizar(
                CodEmpresa,
                request);

        public ErrorDto
            FSL_Expediente_Requisito_Actualizar(
                int CodEmpresa,
                FslExpedienteRequisitoActualizarRequest request)
            => _db.FSL_Expediente_Requisito_Actualizar(
                CodEmpresa,
                request);

        public ErrorDto
            FSL_Expediente_Resolucion_Guardar(
                int CodEmpresa,
                FslExpedienteResolucionGuardarRequest request)
            => _db.FSL_Expediente_Resolucion_Guardar(
                CodEmpresa,
                request);

        public ErrorDto
            FSL_Expediente_Miembro_Validar(
                int CodEmpresa,
                FslExpedienteMiembroValidarRequest request)
            => _db.FSL_Expediente_Miembro_Validar(
                CodEmpresa,
                request);

        public ErrorDto<FslExpedienteAplicarResultado>
            FSL_Expediente_Aplicar(
                int CodEmpresa,
                FslExpedienteAplicarRequest request)
            => _db.FSL_Expediente_Aplicar(
                CodEmpresa,
                request);
    }
}
