using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdRemesasComitesBL
    {
        private readonly FrmAfCdRemesasComitesDB _db;

        public FrmAfCdRemesasComitesBL(IConfiguration config)
        {
            _db = new FrmAfCdRemesasComitesDB(config);
        }

        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_Lista(int codEmpresa)
            => _db.AfCdRemesasTes_Lista(codEmpresa);

        public ErrorDto<bool> AfCdRemesasTes_Guardar(int codEmpresa, AfCdRemesaTesSaveDto dto)
            => _db.AfCdRemesasTes_Guardar(codEmpresa, dto);

        public ErrorDto<bool> AfCdRemesasTes_Eliminar(int codEmpresa, int codRemesa)
            => _db.AfCdRemesasTes_Eliminar(codEmpresa, codRemesa);

        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_ActivasPendientes(int codEmpresa)
            => _db.AfCdRemesasTes_ActivasPendientes(codEmpresa);

        public ErrorDto<AfCdRemesaTesFechasDto> AfCdRemesasTes_Fechas(int codEmpresa, int codRemesa)
            => _db.AfCdRemesasTes_Fechas(codEmpresa, codRemesa);

        public ErrorDto<List<AfCdBancoDto>> AfCdRemesasTes_BancosPorFechas(int codEmpresa, DateTime fechaInicio, DateTime fechaCorte)
            => _db.AfCdRemesasTes_BancosPorFechas(codEmpresa, fechaInicio, fechaCorte);

        public ErrorDto<List<AfCdCuentaOperacionDto>> AfCdRemesasTes_OperacionesPorBanco(
            int codEmpresa, int idBanco, DateTime fechaInicio, DateTime fechaCorte)
            => _db.AfCdRemesasTes_OperacionesPorBanco(codEmpresa, idBanco, fechaInicio, fechaCorte);

        public ErrorDto<List<AfCdCuentaActividadDto>> AfCdRemesasTes_ActividadesPorOperacion(
            int codEmpresa, int noperacion)
            => _db.AfCdRemesasTes_ActividadesPorOperacion(codEmpresa, noperacion);

        public ErrorDto<AfCdRemesaEstadoDto> AfCdRemesasTes_ObtenerEstado(int codEmpresa, int codRemesa, string estado)
            => _db.AfCdRemesasTes_ObtenerEstado(codEmpresa, codRemesa, estado);

        public ErrorDto<AfCdCuentaRemesaDto> AfCdRemesasTes_ObtenerRemesaPorBanco(int codEmpresa, int codRemesa, int idBanco)
            => _db.AfCdRemesasTes_ObtenerRemesaPorBanco(codEmpresa, codRemesa, idBanco);

        public ErrorDto<bool> AfCdRemesasTes_CuentaRemesaSp(int codEmpresa, AfCdCuentaRemesaSpParams param)
            => _db.AfCdRemesasTes_CuentaRemesaSp(codEmpresa, param);

        public ErrorDto<bool> AfCdRemesasTes_ActualizarEstado(int codEmpresa, int codRemesa, string estado)
            => _db.AfCdRemesasTes_ActualizarEstado(codEmpresa, codRemesa, estado);

        public ErrorDto<bool> AfCdCuentas_ActualizarEstadoPorRemesa(int codEmpresa, int codRemesa, string estado)
            => _db.AfCdCuentas_ActualizarEstadoPorRemesa(codEmpresa, codRemesa, estado);
    }
}
