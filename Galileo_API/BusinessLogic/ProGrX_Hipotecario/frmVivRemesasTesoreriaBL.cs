using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivRemesasTesoreriaBL
    {
        private readonly FrmVivRemesasTesoreriaDB _db;

        public FrmVivRemesasTesoreriaBL(IConfiguration config)
        {
            _db = new FrmVivRemesasTesoreriaDB(config);
        }

        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Obtener(int codEmpresa)
            => _db.RemesasTesoreria_Obtener(codEmpresa);

        public ErrorDto<int> RemesasTesoreria_Insertar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
            => _db.RemesasTesoreria_Insertar(codEmpresa, dto);

        public ErrorDto<bool> RemesasTesoreria_Actualizar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
            => _db.RemesasTesoreria_Actualizar(codEmpresa, dto);

        public ErrorDto<bool> RemesasTesoreriaDetalle_Eliminar(int codEmpresa, int remesa)
            => _db.RemesasTesoreriaDetalle_Eliminar(codEmpresa, remesa);

        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Filtrar(int codEmpresa, string tipo)
            => _db.RemesasTesoreria_Filtrar(codEmpresa, tipo);

        public ErrorDto<List<RemesaTesoreriaDesembolsoDisponibleDto>> RemesasTesoreria_DesembolsosDisponibles(int codEmpresa, int remesaSeleccionada)
            => _db.RemesasTesoreria_DesembolsosDisponibles(codEmpresa, remesaSeleccionada);

        public ErrorDto<RemesaTesoreriaExisteDto> RemesasTesoreria_ValidarAbierta(int codEmpresa, int remesaSeleccionada)
            => _db.RemesasTesoreria_ValidarAbierta(codEmpresa, remesaSeleccionada);

        public ErrorDto<bool> RemesasTesoreria_CargarDesembolso(int codEmpresa, int remesaSeleccionada, int codigoDesembolso)
            => _db.RemesasTesoreria_CargarDesembolso(codEmpresa, remesaSeleccionada, codigoDesembolso);

        public ErrorDto<bool> RemesasTesoreria_Cerrar(int codEmpresa, int remesaSeleccionada, string usuario)
            => _db.RemesasTesoreria_Cerrar(codEmpresa, remesaSeleccionada, usuario);

        public ErrorDto<List<RemesaTesoreriaDesembolsoAsignadoDto>> RemesasTesoreria_DesembolsosAsignados(int codEmpresa, int remesaSeleccionada)
            => _db.RemesasTesoreria_DesembolsosAsignados(codEmpresa, remesaSeleccionada);

        public ErrorDto<RemesaTesoreriaExisteDto> RemesasTesoreria_ValidarCerrada(int codEmpresa, int remesaSeleccionada)
            => _db.RemesasTesoreria_ValidarCerrada(codEmpresa, remesaSeleccionada);

        public ErrorDto<bool> RemesasTesoreria_ActualizarProceso(int codEmpresa, int remesaSeleccionada, string usuario, int idDesem)
            => _db.RemesasTesoreria_ActualizarProceso(codEmpresa, remesaSeleccionada, usuario, idDesem);

    }
}
