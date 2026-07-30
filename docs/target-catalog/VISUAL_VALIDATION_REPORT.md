# Relatório de validação visual

## Resultado

**PASS para integridade automatizada e inspeção visual dos artefatos.** Os 26 arquivos abrem e decodificam como JPEG, têm dimensões positivas, não são completamente brancos ou pretos e permanecem dentro de 0,15% da proporção nominal do XML. Nenhum target foi sinalizado como potencialmente pobre em informação pelos limiares abaixo.

Isto não é aprovação de reconhecimento físico. OCR não foi usado como critério.

## Método

- Decodificação completa e `Image.verify()` pelo Pillow.
- Luminância em escala de cinza: média, desvio padrão e extremos.
- Entropia e média de bordas (`FIND_EDGES`) como indicadores auxiliares.
- Sinalização conservadora se desvio padrão < 20, entropia < 4 ou média de bordas < 3.
- Razão de pixels comparada à razão `width/height` do target no XML; tolerância máxima 0,15%.
- Hashes dos JPEGs comparados ao inventário antes e depois da geração.
- Fluxo JPEG incorporado no PDF individual comparado byte a byte ao arquivo-fonte.

## Métricas

| Target | Pixels | Média Y | Desvio Y | Entropia | Borda média | Delta XML | Status |
|---|---:|---:|---:|---:|---:|---:|---|
| A | 1024x721 | 160,334 | 67,926 | 5,845 | 14,309 | 0,081451% | PASS |
| B | 1024x721 | 162,206 | 68,035 | 5,737 | 12,950 | 0,081451% | PASS |
| C | 1024x721 | 167,048 | 67,132 | 5,699 | 13,652 | 0,081451% | PASS |
| D | 1024x721 | 162,898 | 67,360 | 5,839 | 14,138 | 0,081451% | PASS |
| E | 1024x719 | 162,833 | 68,147 | 5,878 | 14,511 | 0,074560% | PASS |
| F | 1024x721 | 166,415 | 68,195 | 5,755 | 14,002 | 0,121934% | PASS |
| G | 1024x721 | 165,275 | 68,747 | 5,840 | 13,844 | 0,081451% | PASS |
| H | 1024x721 | 158,771 | 67,785 | 5,907 | 14,636 | 0,081451% | PASS |
| I | 1024x723 | 171,135 | 67,035 | 5,588 | 13,425 | 0,006463% | PASS |
| J | 1024x721 | 170,010 | 67,735 | 5,627 | 13,619 | 0,121934% | PASS |
| K | 1024x721 | 164,681 | 67,996 | 5,763 | 13,746 | 0,121934% | PASS |
| L | 1024x721 | 167,682 | 68,161 | 5,632 | 13,167 | 0,121934% | PASS |
| M | 1024x721 | 158,600 | 68,314 | 6,008 | 15,105 | 0,121934% | PASS |
| N | 1024x721 | 161,619 | 68,500 | 5,892 | 14,518 | 0,121934% | PASS |
| O | 1024x722 | 169,763 | 68,124 | 5,691 | 13,484 | 0,011774% | PASS |
| P | 1024x722 | 166,479 | 67,380 | 5,749 | 13,963 | 0,011774% | PASS |
| Q | 1024x721 | 164,491 | 67,943 | 5,716 | 13,265 | 0,121934% | PASS |
| R | 1024x721 | 164,812 | 68,268 | 5,766 | 13,996 | 0,121934% | PASS |
| S | 1024x721 | 165,825 | 67,204 | 5,773 | 13,936 | 0,121934% | PASS |
| T | 1024x721 | 168,637 | 67,402 | 5,652 | 13,519 | 0,121934% | PASS |
| U | 1024x721 | 162,266 | 68,324 | 5,784 | 13,791 | 0,121934% | PASS |
| V | 1024x721 | 167,291 | 68,169 | 5,796 | 14,172 | 0,121934% | PASS |
| W | 1024x721 | 156,465 | 68,334 | 5,936 | 14,551 | 0,121934% | PASS |
| X | 1024x721 | 164,746 | 68,022 | 5,764 | 13,758 | 0,121934% | PASS |
| Y | 1024x721 | 169,216 | 67,434 | 5,622 | 13,208 | 0,121934% | PASS |
| Z | 1024x721 | 165,690 | 68,209 | 5,710 | 13,568 | 0,121934% | PASS |

## Inspeção dos artefatos

- `target-contact-sheet.png`: A-Z legíveis, rótulos fora das imagens, sem glifos quebrados.
- PDF 2 por página: 13/13 páginas renderizadas; duas imagens por página, ordem A-Z, margens e intervalo sem sobreposição, marcas de corte externas.
- PDF 1 por página: 26/26 páginas renderizadas; uma imagem centralizada por página, sem deformação.
- PDF contato: 1/1 página renderizada; 26 miniaturas e aviso explícito de que não serve para tracking.
- Fontes Vera/Vera Bold incorporadas; renderização final sem avisos de substituição.

