using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de las Apelaciones de Expediente Fosol (frmFSL_ExpedienteApelaciones).
    /// </summary>
    public class FrmFslExpedienteApelacionesBL
    {
        private readonly FrmFslExpedienteApelacionesDB _db;

        public FrmFslExpedienteApelacionesBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslExpedienteApelacionesDB(config);
        }

        /// <summary>Tipos de apelación activos.</summary>
        public ErrorDto<List<FslTipoApelacion>> FslTipoApelacion_Obtener(int CodCliente)
            => _db.FslTipoApelacion_Obtener(CodCliente);

        /// <summary>Registra una apelación al expediente.</summary>
        public ErrorDto FslApelacion_Aplicar(int CodCliente, FslApleacionAplicar expediente)
            => _db.FslApelacion_Aplicar(CodCliente, expediente);

        /// <summary>Aplica la resolución de una apelación.</summary>
        public ErrorDto FslResolucionApelacion_Aplicar(int CodCliente, string apelacion)
            => _db.FslResolucionApelacion_Aplicar(CodCliente, apelacion);
    }
}
