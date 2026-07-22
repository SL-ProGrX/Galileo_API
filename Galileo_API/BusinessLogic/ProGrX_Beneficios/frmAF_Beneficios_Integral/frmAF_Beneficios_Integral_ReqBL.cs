using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.GA;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Requisitos de Beneficios Integrales (FrmAfBeneficiosIntegralReq).
    /// </summary>
    public class FrmAfBeneficiosIntegralReqBL
    {
        private readonly FrmAfBeneficiosIntegralReqDB _db;

        public FrmAfBeneficiosIntegralReqBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralReqDB(config);
        }

        /// <summary>Lista de requisitos del beneficio.</summary>
        public ErrorDto<List<BeneRegRequisito>> Bene_Reg_Requisitos_Obtener(int CodCliente, int consec)
            => _db.Bene_Registro_Requisitos_Obtener(CodCliente, consec);

        /// <summary>Registra un requisito del beneficio.</summary>
        public ErrorDto BeneRegistroRequisitos_Guardar(BeneRequisitosGuardar requisito)
            => _db.BeneRegistroRequisitos_Guardar(requisito);

        /// <summary>Elimina un requisito del beneficio.</summary>
        public ErrorDto BeneRegistroRequisitos_Eliminar(int CodCliente, string cod_beneficio, int consec, string cod_requisito, string usuario)
            => _db.BeneRegistroRequisitos_Eliminar(CodCliente, cod_beneficio, consec, cod_requisito, usuario);

        /// <summary>Asocia el archivo GA a un requisito del beneficio.</summary>
        public ErrorDto BeneRegistroRequisito_Asociar(string modulo, string TypeId, string requisito, DocumentosArchivoDto data)
            => _db.BeneRegistroRequisito_Asociar(modulo, TypeId, requisito, data);
    }
}
