using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los Parámetros de Beneficios Fosol (frmFSL_Parametros).
    /// </summary>
    public class FrmFslParametrosBL
    {
        private readonly FrmFslParametrosDB _db;

        public FrmFslParametrosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslParametrosDB(config);
        }

        /// <summary>Lista de parámetros Fosol.</summary>
        public ErrorDto<FdlParametrosListaDto> FslParametros_Obtener(int CodCliente, string filtros)
            => _db.FslParametros_Obtener(CodCliente, filtros);

        /// <summary>Actualiza el valor de un parámetro Fosol.</summary>
        public ErrorDto FslParametros_Actualizar(int CodCliente, FdlParametrosDto parametro)
            => _db.FslParametros_Actualizar(CodCliente, parametro);
    }
}
