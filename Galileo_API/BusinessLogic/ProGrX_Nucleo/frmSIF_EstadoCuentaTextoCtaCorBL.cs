using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.BusinessLogic
{
    public class FrmSifEstadoCuentaTextoCtaCorBL(IConfiguration config) 
    {
        private readonly FrmSifEstadoCuentaTextoCtaCorDB _db = new FrmSifEstadoCuentaTextoCtaCorDB(config);

        public ErrorDto<SifEmpresaDto> NotasEstados_Obtener(int CodEmpresa)
        {
            return _db.NotasEstados_Obtener(CodEmpresa);
        }

        public ErrorDto NotasEstados_Insertar(int CodCliente, SifEmpresaDto notas)
        {
            return _db.NotasEstados_Insertar(CodCliente, notas);
        }

    }
}