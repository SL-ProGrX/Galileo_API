using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Newtonsoft.Json;

    namespace Galileo_API.BusinessLogic.ProGrX_Comites
    {
        public class FrmTipoArchivoBL
        {
            private readonly FrmTipoArchivoDB _db;

            public FrmTipoArchivoBL(IConfiguration config)
            {
                _db = new FrmTipoArchivoDB(config);
            }

            public ErrorDto<TipoArchivoLista> TipoArchivoLista_Obtener(int CodEmpresa, string jfiltros)
            {
                FiltrosLazyLoadData filtros =
                    JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)
                    ?? new FiltrosLazyLoadData();

                return _db.TipoArchivoLista_Obtener(CodEmpresa, filtros);
            }

            public ErrorDto TipoArchivo_Guardar(int CodEmpresa, string usuario, TipoArchivoData data)
            {
                return _db.TipoArchivo_Guardar(CodEmpresa, usuario, data);
            }

            public ErrorDto TipoArchivo_Eliminar(int CodEmpresa, int id, string usuario)
            {
                return _db.TipoArchivo_Eliminar(CodEmpresa, id, usuario);
            }

            public ErrorDto<List<TipoArchivoData>> TipoArchivo_Obtener(int CodEmpresa, string jfiltros)
            {
                FiltrosLazyLoadData filtros =
                    JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)
                    ?? new FiltrosLazyLoadData();

                return _db.TipoArchivo_Obtener(CodEmpresa, filtros);
            }
        }
    }

