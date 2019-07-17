using Calico.common;
using Calico.persistencia;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calico.interfaces.items
{
    class ItemUtils
    {

        public String BuildUrl(String urlParam, String fecha)
        {
            Dictionary<String, String> dictionary = new Dictionary<string, string>();
            dictionary.Add(Constants.PARAM_FECHA, fecha);
            return Utils.BuildUrl(urlParam, dictionary);
        }

        public List<ItemDTO> MappingJsonRecepcion(String myJsonString)
        {
            var jc = JsonConvert.DeserializeObject<JObject>(myJsonString);

            JArray rowset = jc.Value<JObject>(Constants.JSON_PREFIX + Constants.JSON_SUBFIX_ITEM)
                                 .Value<JObject>(Constants.JSON_TAG_DATA)
                                 .Value<JObject>(Constants.JSON_TAG_GRIDDATA)
                                 .Value<JArray>(Constants.JSON_TAG_ROWSET);

            if (rowset != null && rowset.Count > 0)
            {
                return rowset.ToObject<List<ItemDTO>>() as List<ItemDTO>;
            }

            return new List<ItemDTO>();
        }

        public void MappingReceptionDTORecepcion(List<ItemDTO> itemDTOList, Dictionary<String, tblProducto> dictionary)
        {
            foreach (ItemDTO item in itemDTOList)
            {
                tblProducto producto = new tblProducto();
                String compania = !String.IsNullOrWhiteSpace(item.F4101_SRP1) ? item.F4101_SRP1.Trim() : String.Empty;
                producto.prod_compania = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_ITEMS + "." + Constants.COMPANIA, compania);
                producto.prod_codigo = !String.IsNullOrWhiteSpace(item.F4101_LITM) ? item.F4101_LITM.Trim() : String.Empty;
                producto.prod_descripcion = !String.IsNullOrWhiteSpace(item.F4101_DSC1) ? item.F4101_DSC1.Trim() : String.Empty;
                producto.prod_descripcionFantasia = !String.IsNullOrWhiteSpace(item.F4101_DSC2) ? item.F4101_DSC2.Trim() : String.Empty;
                producto.prod_unidadMedida = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_ITEMS, Constants.UNIDAD_MEDIDA);
                producto.prod_categoriaUbicacion = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_ITEMS, Constants.CATEGORIA_UBICACION);
                producto.prod_categoriaPicking = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_ITEMS, Constants.CATEGORIA_PICKING);
                producto.prod_categoriaReposicion = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_ITEMS, Constants.CATEGORIA_REPOSICION);
                producto.prod_categoriaEmbalaje = FilePropertyUtils.Instance.GetValueString(Constants.INTERFACE_ITEMS, Constants.CATEGORIA_EMBALAJE);
                dictionary.Add(producto.prod_codigo, producto);
            }
        }

    }
}
