using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHExcedentesEncargadosBL
    {
        private readonly FrmAHExcedentesEncargadosDB _db;

        public FrmAHExcedentesEncargadosBL(IConfiguration config)
        {
            _db = new FrmAHExcedentesEncargadosDB(config);
        }

        /// <summary>
        /// Obtiene la lista completa de encargados de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<EncargadoExcedenteDto>>
            AH_Excedentes_Encargados_Lista_Obtener(int CodEmpresa)
        {
            return _db
                .AH_Excedentes_Encargados_Lista_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Registra o actualiza un encargado de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="encargado"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Encargados_Guardar(int CodEmpresa,string usuario, EncargadoExcedenteDto encargado)
        {
            return _db.AH_Excedentes_Encargados_Guardar(CodEmpresa,usuario,encargado);
        }

        /// <summary>
        /// Elimina un encargado de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioEncargado"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Encargados_Eliminar(int CodEmpresa,string usuarioEncargado,string usuario)
        {
            return _db.AH_Excedentes_Encargados_Eliminar(CodEmpresa,usuarioEncargado,usuario);
        }

        /// <summary>
        /// Obtiene los usuarios activos disponibles para seleccionar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Encargados_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.AH_Excedentes_Encargados_Usuarios_Dropdown_Obtener(CodEmpresa);
        }
    }
}