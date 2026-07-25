using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de la Carga por Lote de Beneficios (frmAF_BeneficiosCargaLote).
    /// </summary>
    public class FrmAfBeneficiosCargaLoteBL
    {
        private readonly FrmAfBeneficiosCargaLoteDB _db;

        public FrmAfBeneficiosCargaLoteBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosCargaLoteDB(config);
        }

        /// <summary>Inserta un lote de beneficios.</summary>
        public ErrorDto Beneficio_Lote_Carga_Insertar(int CodEmpresa, string beneficio)
            => _db.Beneficio_Lote_Carga_Insertar(CodEmpresa, beneficio);

        /// <summary>Obtiene la revisión de un lote cargado.</summary>
        public ErrorDto<List<AfiBeneCargaLoteData>> Beneficio_Lote_Revisa_Obtener(int CodEmpresa, string cod_beneficio, string usuario)
            => _db.Beneficio_Lote_Revisa_Obtener(CodEmpresa, cod_beneficio, usuario);

        /// <summary>Procesa un lote de beneficios.</summary>
        public ErrorDto Beneficio_Lote_Procesa(int CodEmpresa, string cod_beneficio, string usuario, string Formato)
            => _db.Beneficio_Lote_Procesa(CodEmpresa, cod_beneficio, usuario, Formato);
    }
}
