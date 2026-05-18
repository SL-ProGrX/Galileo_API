using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdInformeEspecialDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdInformeEspecialDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmAfCdInformeEspecialDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene la informacion inicial real de la pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<AfCdInformeEspecialPantallaData> AfCdInformeEspecial_Pantalla_Obtener(int codEmpresa)
        {
            var respZonas = AfCdInformeEspecial_Zonas_Obtener(codEmpresa);
            if (respZonas.Code < 0)
            {
                return new ErrorDto<AfCdInformeEspecialPantallaData>
                {
                    Code = -1,
                    Description = respZonas.Description
                };
            }

            var respComites = AfCdInformeEspecial_Comites_Obtener(codEmpresa, string.Empty);
            if (respComites.Code < 0)
            {
                return new ErrorDto<AfCdInformeEspecialPantallaData>
                {
                    Code = -1,
                    Description = respComites.Description
                };
            }

            var respActividades = AfCdInformeEspecial_Actividades_Obtener(codEmpresa);
            if (respActividades.Code < 0)
            {
                return new ErrorDto<AfCdInformeEspecialPantallaData>
                {
                    Code = -1,
                    Description = respActividades.Description
                };
            }

            var respAntiguedad = AfCdInformeEspecial_Antiguedad_Obtener(codEmpresa);
            if (respAntiguedad.Code < 0)
            {
                return new ErrorDto<AfCdInformeEspecialPantallaData>
                {
                    Code = -1,
                    Description = respAntiguedad.Description
                };
            }

            return new ErrorDto<AfCdInformeEspecialPantallaData>
            {
                Code = 0,
                Result = new AfCdInformeEspecialPantallaData
                {
                    zonas = respZonas.Result ?? new List<DropDownListaGenericaModel>(),
                    comites = respComites.Result ?? new List<DropDownListaGenericaModel>(),
                    actividades = respActividades.Result ?? new List<DropDownListaGenericaModel>(),
                    antiguedad = respAntiguedad.Result ?? new List<DropDownListaGenericaModel>(),
                }
            };
        }

        /// <summary>
        /// Obtiene las zonas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Zonas_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    COD_ZONA as item,
                    rtrim(DESCRIPCION) as descripcion
                from AFI_ZONAS
                order by DESCRIPCION";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query
            );
        }

        /// <summary>
        /// Obtiene los comites segun la zona seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codZona"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Comites_Obtener(int codEmpresa, string codZona)
        {
            codZona = (codZona ?? string.Empty).Trim();

            string query;
            object? parametros = null;

            if (string.IsNullOrWhiteSpace(codZona) ||
                codZona.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
            {
                query = @"
                    select
                        COD_COMITE as item,
                        rtrim(DESCRIPCION) as descripcion
                    from AFI_CD_COMITES
                    order by DESCRIPCION";
            }
            else
            {
                query = @"
                    select
                        COD_COMITE as item,
                        rtrim(DESCRIPCION) as descripcion
                    from vAFI_CD_Comites_Zonas
                    where COD_ZONA = @CodZona
                    order by DESCRIPCION";

                parametros = new { CodZona = codZona };
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                parametros
            );
        }

        /// <summary>
        /// Obtiene las unidades del comite seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Unidades_Obtener(int codEmpresa, string codComite)
        {
            codComite = (codComite ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codComite) ||
                codComite.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = 0,
                    Result = new List<DropDownListaGenericaModel>()
                };
            }

            const string query = @"
                select
                    U.CODIGO as item,
                    U.DESCRIPCION as descripcion
                from AFI_CD_COMITES_UNIDADES CU
                inner join UPROGRAMATICA U
                    on CU.CODIGO_UP = U.CODIGO
                where CU.COD_COMITE = @CodComite
                order by U.CODIGO";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new { CodComite = codComite }
            );
        }

        /// <summary>
        /// Obtiene actividades activas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Actividades_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    cast(COD_ACTIVIDAD as varchar(20)) as item,
                    rtrim(DESCRIPCION) as descripcion
                from AFI_CD_ACTIVIDADES
                where ACTIVA = 1
                order by DESCRIPCION";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query
            );
        }

        /// <summary>
        /// Obtiene tipos de antiguedad.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Antiguedad_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    cast(COD_ANTIGUEDAD as varchar(20)) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CBR_ANTIGUEDAD_TIPOS
                order by DESCRIPCION";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query
            );
        }
    }
}
