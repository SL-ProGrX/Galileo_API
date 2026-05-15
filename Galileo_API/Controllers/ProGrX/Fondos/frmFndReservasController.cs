using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndReservasController : ControllerBase
    {
        private readonly FrmFndReservasBl BlFndReservas;

        public FrmFndReservasController(IConfiguration config)
        {
            BlFndReservas = new FrmFndReservasBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_Reservas_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_Reservas_Obtener(int CodEmpresa, bool Exporta, string Filtros)
        {
            return BlFndReservas.Fnd_Reservas_Obtener(CodEmpresa, Exporta, Filtros);
        }

        [Authorize]
        [HttpGet("Fnd_Reservas_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Reservas_Catalogo_Obtener(int CodEmpresa, int TabIndex)
        {
            return BlFndReservas.Fnd_Reservas_Catalogo_Obtener(CodEmpresa, TabIndex);
        }

        [Authorize]
        [HttpGet("Fnd_Reservas_Cuentas_Obtener")]
        public ErrorDto<List<FndReservaCuentaDto>> Fnd_Reservas_Cuentas_Obtener(int CodEmpresa, string Reserva)
        {
            return BlFndReservas.Fnd_Reservas_Cuentas_Obtener(CodEmpresa, Reserva);
        }

        [Authorize]
        [HttpGet("Fnd_Reservas_Contenido_Obtener")]
        public ErrorDto<List<FndReservaContenidoDto>> Fnd_Reservas_Contenido_Obtener(int CodEmpresa, string Reserva)
        {
            return BlFndReservas.Fnd_Reservas_Contenido_Obtener(CodEmpresa, Reserva);
        }

        [Authorize]
        [HttpGet("Fnd_Reservas_Cortes_Obtener")]
        public ErrorDto<List<FndReservaCorteDto>> Fnd_Reservas_Cortes_Obtener(int CodEmpresa, string Filtros)
        {
            return  BlFndReservas.Fnd_Reservas_Cortes_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpGet("Fnd_Reservas_Saldo_Obtener")]
        public ErrorDto<decimal> Fnd_Reservas_Saldo_Obtener(int CodEmpresa, string Reserva, string FechaInicio)
        {
            return BlFndReservas.Fnd_Reservas_Saldo_Obtener(CodEmpresa, Reserva, FechaInicio);
        }

        [Authorize]
        [HttpPost("Fnd_Reservas_Guardar")]
        public ErrorDto Fnd_Reservas_Guardar(int CodEmpresa, FndReservasDto Data)
        {
            return BlFndReservas.Fnd_Reservas_Guardar(CodEmpresa, Data);
        }

        [Authorize]
        [HttpDelete("Fnd_Reservas_Eliminar")]
        public ErrorDto Fnd_Reservas_Eliminar(int CodEmpresa, string Reserva, string Usuario)
        {
            return BlFndReservas.Fnd_Reservas_Eliminar(CodEmpresa, Reserva, Usuario);
        }

        [Authorize]
        [HttpPost("Fnd_Reservas_Cuentas_Registro")]
        public ErrorDto Fnd_Reservas_Cuentas_Registro(int CodEmpresa, string Reserva, string CodCuenta, string Usuario, string Accion)
        {
            return BlFndReservas.Fnd_Reservas_Cuentas_Registro(CodEmpresa, Reserva, CodCuenta, Usuario, Accion);
        }

        [Authorize]
        [HttpPost("Fnd_Reservas_Mov_Registro")]
        public ErrorDto Fnd_Reservas_Mov_Registro(int CodEmpresa, string Reserva, string Usuario, int Accion, FndReservaContenidoDto Filtros)
        {
            return BlFndReservas.Fnd_Reservas_Mov_Registro(CodEmpresa, Reserva, Usuario, Accion, Filtros);
        }
    }
}