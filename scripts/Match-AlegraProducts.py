import csv,json,re,unicodedata,argparse
from pathlib import Path
from difflib import SequenceMatcher
p=argparse.ArgumentParser();p.add_argument('directory');args=p.parse_args();d=Path(args.directory)
def norm(s):
 s=''.join(c for c in unicodedata.normalize('NFKD',s.upper()) if not unicodedata.combining(c))
 return ' '.join(re.sub(r'[^A-Z0-9]+',' ',s).split())
local=list(csv.DictReader((d/'productos-locales.csv').open(encoding='utf-8-sig')))
remote=json.loads((d/'productos-alegra.json').read_text(encoding='utf-8-sig'))
active=[r for r in remote if str(r.get('status','active')).lower()!='inactive']
rows=[]
for l in local:
 if (l['Estado'] or 'ACTIVO').strip().upper()!='ACTIVO':continue
 n=norm(l['Nombre']);exact=[r for r in active if r['name'].strip().casefold()==l['Nombre'].strip().casefold()]
 normalized=[r for r in active if norm(r['name'])==n]
 candidates=exact or normalized
 kind='EXACTA_UNICA' if len(exact)==1 else 'EXACTA_DUPLICADA' if exact else 'NORMALIZADA_UNICA' if len(normalized)==1 else 'NORMALIZADA_DUPLICADA' if normalized else 'SIMILITUD_REVISAR'
 scored=sorted(((SequenceMatcher(None,n,norm(r['name'])).ratio(),r) for r in active),key=lambda v:v[0],reverse=True)
 if not candidates and scored[0][0]<0.65:kind='SIN_COINCIDENCIA_CLARA'
 selected=[(1.0,r) for r in candidates] if candidates else scored[:3]
 row={'Tipo':l['Tipo'],'IdLocal':l['Id'],'NombreLocal':l['Nombre'],'Clasificacion':kind,'IdAlegraElegido':'','Decision':'','Observaciones':''}
 for i,(score,r) in enumerate(selected[:3],1):row.update({f'Candidato{i}Id':r['id'],f'Candidato{i}Nombre':r['name'],f'Candidato{i}Similitud':round(score*100,1)})
 if len(candidates)>3:row['Observaciones']=f'{len(candidates)} candidatos; consultar catalogo completo'
 if kind=='EXACTA_UNICA':row['IdAlegraElegido']=str(exact[0]['id']);row['Decision']='PROPUESTA_EXACTA'
 rows.append(row)
columns=['Tipo','IdLocal','NombreLocal','Clasificacion','IdAlegraElegido','Decision','Observaciones']+[f'Candidato{i}{suffix}' for i in range(1,4) for suffix in ['Id','Nombre','Similitud']]
for filename,data in [('inventario-completo.csv',rows),('conflictos-por-resolver.csv',[r for r in rows if r['Clasificacion']!='EXACTA_UNICA'])]:
 with (d/filename).open('w',encoding='utf-8-sig',newline='') as f:
  w=csv.DictWriter(f,fieldnames=columns,delimiter=';');w.writeheader();w.writerows(data)
from collections import Counter
stats=Counter(r['Clasificacion'] for r in rows)
(d/'resumen.json').write_text(json.dumps({'locales_total':len(local),'locales_activos':len(rows),'locales_inactivos':len(local)-len(rows),'alegra_total':len(remote),'alegra_activos':len(active),'clasificaciones':stats},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(stats,ensure_ascii=False))
