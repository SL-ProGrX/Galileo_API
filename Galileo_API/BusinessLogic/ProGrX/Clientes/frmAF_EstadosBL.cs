using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFEstadosBL
    {
        private readonly FrmAFEstadosDB _db;

        public FrmAFEstadosBL(IConfiguration config)
        {
            _db = new FrmAFEstadosDB(config);
        }

        public ErrorDto<AfEstadosLista> AF_Estados_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_Estados_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_Estados_Guardar(int CodEmpresa, string Usuario, AfEstadosDto Info)
        {
            return _db.AF_Estados_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_Estados_Eliminar(int CodEmpresa, string Usuario, string CodEstado)
        {
            return _db.AF_Estados_Eliminar(CodEmpresa, Usuario, CodEstado);
        }

        public ErrorDto<List<AfEstadosMovimientosDto>> AF_Estados_Movimientos_Obtener(int CodEmpresa)
        {
            return _db.AF_Estados_Movimientos_Obtener(CodEmpresa);
        }

        public ErrorDto AF_Estados_Movimientos_Registrar(int CodEmpresa, AfEstadosMovimientosDto Info)
        {
            return _db.AF_Estados_Movimientos_Registrar(CodEmpresa, Info);
        }

        public ErrorDto AF_Estados_Movimientos_Eliminar(int CodEmpresa, string Lista)
        {
            List<AfEstadosMovimientosDto> list = JsonConvert.DeserializeObject<List<AfEstadosMovimientosDto>>(Lista) ?? new List<AfEstadosMovimientosDto>();
            return _db.AF_Estados_Movimientos_Eliminar(CodEmpresa, list);
        }

        public ErrorDto<List<AfEstadosEntidadesDto>> AF_Estados_Entidades_Obtener(int CodEmpresa, string CodEstado)
        {
            return _db.AF_Estados_Entidades_Obtener(CodEmpresa, CodEstado);
        }

        public ErrorDto AF_Estados_Entidad_Guardar(int CodEmpresa, string Usuario, AfEstadosEntidadesDto Info)
        {
            return _db.AF_Estados_Entidad_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_Estados_EntidadesTodas_Guardar(int CodEmpresa, string Usuario, string CodEstado, bool Checked)
        {
            return _db.AF_Estados_EntidadesTodas_Guardar(CodEmpresa, Usuario, CodEstado, Checked);
        }
    }
}