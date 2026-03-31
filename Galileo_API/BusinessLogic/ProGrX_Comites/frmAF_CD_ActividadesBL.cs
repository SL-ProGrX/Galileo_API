using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdActividadesBL
    {
        private readonly FrmAfCdActividadesDB _db;

        public FrmAfCdActividadesBL(IConfiguration config)
        {
            _db = new FrmAfCdActividadesDB(config);
        }

        public ErrorDto<List<AfCdActividadDto>> AfCdActividades_Lista(int codEmpresa, int codContabilidad)
            => _db.AfCdActividades_Lista(codEmpresa, codContabilidad);

        public ErrorDto<bool> AfCdActividades_Upsert(int codEmpresa, AfCdActividadDto dto)
            => _db.AfCdActividades_Upsert(codEmpresa, dto);

        public ErrorDto<List<AfCdActividadComiteDto>> AfCdActividades_ComitesPorActividad(int codEmpresa, int codActividad)
            => _db.AfCdActividades_ComitesPorActividad(codEmpresa, codActividad);

        public ErrorDto<bool> AfCdActividades_EliminarComitesPorActividad(int codEmpresa, int codActividad)
            => _db.AfCdActividades_EliminarComitesPorActividad(codEmpresa, codActividad);

        public ErrorDto<List<AfCdActividadSimpleDto>> AfCdActividades_SimpleLista(int codEmpresa)
            => _db.AfCdActividades_SimpleLista(codEmpresa);

        public ErrorDto<List<AfCdActividadRangoDto>> AfCdActividades_RangosPorActividad(int codEmpresa, int codActividad)
            => _db.AfCdActividades_RangosPorActividad(codEmpresa, codActividad);

        public ErrorDto<bool> AfCdActividades_RangoUpsert(int codEmpresa, int codActividad, AfCdActividadRangoDto dto)
            => _db.AfCdActividades_RangoUpsert(codEmpresa, codActividad, dto);

        public ErrorDto<bool> AfCdActividades_RangoDelete(int codEmpresa, int codActividad, int codMonto)
            => _db.AfCdActividades_RangoDelete(codEmpresa, codActividad, codMonto);

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdActividades_DropDownLista(int codEmpresa)
            => _db.AfCdActividades_DropDownLista(codEmpresa);

        public ErrorDto<List<AfCdCuentaConsultaDto>> AfCdCuentas_Consulta(int codEmpresa, DateTime fechaInicio, DateTime fechaFin, string codActividad)
            => _db.AfCdCuentas_Consulta(codEmpresa, fechaInicio, fechaFin, codActividad);
    }
}
