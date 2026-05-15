using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndReservasBl
    {
        private readonly FrmFndReservasDb DbFndReservas;

        public FrmFndReservasBl(IConfiguration config)
        {
            DbFndReservas = new FrmFndReservasDb(config);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_Reservas_Obtener(int CodEmpresa, bool Exporta, string Filtros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(Filtros) ?? new FiltrosLazyLoadData();
            return DbFndReservas.Fnd_Reservas_Obtener(CodEmpresa, Exporta, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Reservas_Catalogo_Obtener(int CodEmpresa, int TabIndex)
        {
            return DbFndReservas.Fnd_Reservas_Catalogo_Obtener(CodEmpresa, TabIndex);
        }

        public ErrorDto<List<FndReservaCuentaDto>> Fnd_Reservas_Cuentas_Obtener(int CodEmpresa, string Reserva)
        {
            return DbFndReservas.Fnd_Reservas_Cuentas_Obtener(CodEmpresa, Reserva);
        }

        public ErrorDto<List<FndReservaContenidoDto>> Fnd_Reservas_Contenido_Obtener(int CodEmpresa, string Reserva)
        {
            return DbFndReservas.Fnd_Reservas_Contenido_Obtener(CodEmpresa, Reserva);
        }

        public ErrorDto<List<FndReservaCorteDto>> Fnd_Reservas_Cortes_Obtener(int CodEmpresa, string Filtros)
        {
            FndReservaCorteFiltros filtros = JsonConvert.DeserializeObject<FndReservaCorteFiltros>(Filtros) ?? new FndReservaCorteFiltros();
            return DbFndReservas.Fnd_Reservas_Cortes_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<decimal> Fnd_Reservas_Saldo_Obtener(int CodEmpresa, string Reserva, string FechaInicio)
        {
            return DbFndReservas.Fnd_Reservas_Saldo_Obtener(CodEmpresa, Reserva, FechaInicio);
        }

        public ErrorDto Fnd_Reservas_Guardar(int CodEmpresa, FndReservasDto Data)
        {
            return DbFndReservas.Fnd_Reservas_Guardar(CodEmpresa, Data);
        }

        public ErrorDto Fnd_Reservas_Eliminar(int CodEmpresa, string Reserva, string Usuario)
        {
            return DbFndReservas.Fnd_Reservas_Eliminar(CodEmpresa, Reserva, Usuario);
        }

        public ErrorDto Fnd_Reservas_Cuentas_Registro(int CodEmpresa, string Reserva, string CodCuenta, string Usuario, string Accion)
        {
            return DbFndReservas.Fnd_Reservas_Cuentas_Registro(CodEmpresa, Reserva, CodCuenta, Usuario, Accion);
        }

        public ErrorDto Fnd_Reservas_Mov_Registro(int CodEmpresa, string Reserva, string Usuario, int Accion, FndReservaContenidoDto Filtros)
        {
            return DbFndReservas.Fnd_Reservas_Mov_Registro(CodEmpresa, Reserva, Usuario, Accion, Filtros);
        }
    }
}