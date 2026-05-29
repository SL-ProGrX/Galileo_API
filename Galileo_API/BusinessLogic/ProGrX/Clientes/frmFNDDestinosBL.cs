using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmFndDestinosBL
    {
        private readonly FrmFndDestinosDB _db;

        public FrmFndDestinosBL(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _db = new FrmFndDestinosDB(config);
        }

        public ErrorDto<List<FndDestinosData>> Fnd_Destinos_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Destinos_Obtener(CodEmpresa);
        }

        public ErrorDto<FndDestinosLista> Fnd_DestinosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return _db.Fnd_DestinosLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<FndPlanesDestinoLista> Fnd_PlanesLista_Obtener(int CodEmpresa, string Cod_Destino, FiltrosLazyLoadData filtros)
        {
            return _db.Fnd_PlanesLista_Obtener(CodEmpresa, Cod_Destino, filtros);
        }

        public ErrorDto Fnd_Destinos_Valida(int CodEmpresa, string codDestino)
        {
            return _db.Fnd_Destinos_Valida(CodEmpresa, codDestino);
        }

        public ErrorDto Fnd_Destinos_Guardar(int CodEmpresa, string usuario, FndDestinosData destino)
        {
            return _db.Fnd_Destinos_Guardar(CodEmpresa, usuario, destino);
        }

        public ErrorDto Fnd_Destinos_Eliminar(int CodEmpresa, string usuario, string codDestino)
        {
            return _db.Fnd_Destinos_Eliminar(CodEmpresa, usuario, codDestino);
        }

        public ErrorDto<List<FndPlanesDestinoData>> Fnd_Planes_Obtener(int CodEmpresa, string codDestino)
        {
            return _db.Fnd_Planes_Obtener(CodEmpresa, codDestino);
        }

        public ErrorDto Fnd_Planes_AsignarDesasignar(int CodEmpresa, FndAsignarPlanRequest request)
        {
            return _db.Fnd_Planes_AsignarDesasignar(CodEmpresa, request);
        }
    }
}