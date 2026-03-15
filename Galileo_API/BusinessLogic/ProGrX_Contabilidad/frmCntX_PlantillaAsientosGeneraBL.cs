using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaAsientosGeneraBL
    {
        private readonly FrmCntXPlantillaAsientosGeneraDB _db;

        public FrmCntXPlantillaAsientosGeneraBL(IConfiguration config)
        {
            _db = new FrmCntXPlantillaAsientosGeneraDB(config);
        }

        public ErrorDto<List<CntXPlantillaAsientosDto>> CntXPlantillaAsientos_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXPlantillaAsientos_Lista(codEmpresa, codContabilidad);

        public ErrorDto<CntXPlantillaAsientosDto?> CntXPlantillaAsientos_Get(int codEmpresa, int codContabilidad, string codPlantilla)
            => _db.CntXPlantillaAsientos_Get(codEmpresa, codContabilidad, codPlantilla);

        public ErrorDto<bool> CntXPlantillaAsientos_UpdateConsecutivo(int codEmpresa, CntXPlantillaAsientosUpdateParams param)
            => _db.CntXPlantillaAsientos_UpdateConsecutivo(codEmpresa, param);

        public ErrorDto<bool> CntxAsientos_Insert(int codEmpresa, CntxAsientosInsertParams param)
            => _db.CntxAsientos_Insert(codEmpresa, param);

        public ErrorDto<List<CntXPlantillaDetalleDto>> CntXPlantillaDetalle_Lista(int codEmpresa, int codContabilidad, string codPlantilla)
            => _db.CntXPlantillaDetalle_Lista(codEmpresa, codContabilidad, codPlantilla);

        public ErrorDto<bool> CntxAsientosDetalle_Insert(int codEmpresa, CntxAsientosDetalleInsertParams param)
            => _db.CntxAsientosDetalle_Insert(codEmpresa, param);

        public ErrorDto<int> CntXPeriodos_ExisteAbierto(int codEmpresa, int codContabilidad, int anio, int mes)
             => _db.CntXPeriodos_ExisteAbierto(codEmpresa, codContabilidad, anio, mes);
    }
}
