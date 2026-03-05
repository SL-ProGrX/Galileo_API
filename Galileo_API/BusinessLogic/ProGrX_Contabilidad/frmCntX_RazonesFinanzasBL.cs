using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRazonesFinanzasBL
    {
        private readonly FrmCntXRazonesFinanzasDB _db;

        public FrmCntXRazonesFinanzasBL(IConfiguration config)
        {
            _db = new FrmCntXRazonesFinanzasDB(config);
        }

        public ErrorDto<List<CntXRazonesFinanzasDto>> CntXRazonesFinanzas_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXRazonesFinanzas_Lista(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXRazonesFinanzas_Existe(int codEmpresa, int codContabilidad)
            => _db.CntXRazonesFinanzas_Existe(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXRazonesFinanzas_Guardar(int codEmpresa, CntXRazonesFinanzasSaveParams param)
            => _db.CntXRazonesFinanzas_Guardar(codEmpresa, param);

        public ErrorDto<List<CntXRazonFinancieraDto>> CntXRazonFinanciera_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXRazonFinanciera_Lista(codEmpresa, codContabilidad);

        public ErrorDto<List<CntXRazonFinancieraTipoDto>> CntXRazonFinancieraTipos_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXRazonFinancieraTipos_Lista(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXRazonFinanciera_Guardar(int codEmpresa, CntXRazonFinancieraSaveParams param)
            => _db.CntXRazonFinanciera_Guardar(codEmpresa, param);
    }
}
