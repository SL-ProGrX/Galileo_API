using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic
{
    public class FrmAFBeneficiariosBL
    {
        private readonly FrmAFBeneficiariosDB _db;

        public FrmAFBeneficiariosBL(IConfiguration config)
        {
            _db = new FrmAFBeneficiariosDB(config);
        }

        public ErrorDto<List<PersonaBeneficiarioDto>> AF_PersonaBeneficiarios_Consulta(int CodEmpresa, string cedula, int? lineaId)
        {
            return _db.AF_PersonaBeneficiarios_Consulta(CodEmpresa, cedula, lineaId);
        }

        public ErrorDto<int> AF_PersonaBeneficiarios_Registro(int CodEmpresa, PersonaBeneficiarioDto dto)
        {
            return _db.AF_PersonaBeneficiarios_Registro(CodEmpresa, dto);
        }

        public ErrorDto<BeneficiariosCatalogoDto> AF_Beneficiarios_Catalogos_Obtener(int CodEmpresa)
        {
            return _db.AF_Beneficiarios_Catalogos_Obtener(CodEmpresa);
        }
    }
}