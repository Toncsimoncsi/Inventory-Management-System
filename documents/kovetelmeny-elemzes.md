# Követelmény feltárás és elemzés

## Menedzselési követelmények
### Környezeti (environmental)
- nem működik együtt semmilyen külső szoftverrel, szolgáltatással

### Működési (operational)
- tetszőleges ideig használható
- gyakori használat

### Fejlesztési (development)
- C# nyelv, WPF .NET keretrendszer
- MVVM architektúra
- objektumorientált paradigma

## Külső követelmények
### Megbízhatóság
- szabványos használat esetén, szabályos szimuláció létrehozásánál nem fordul elő hibajelenség, nem jelenik meg hibaüzenet
- amennyiben bármely adat megsérül, vagy a program használata nélkül módosul, adatvesztés léphet fel, amely érintheti az összes addigi adatot (de a funkciók használatát nem)
- hibás programműködés esetén is léphet fel adatvesztés

### Hatékonyság
- jelentéktelen terhelés a processzor részére, hálózatot nem igényel
- a memória, illetve merevlemez terhelés nem jelentős
- gyors (>1mp) válaszidő minden bevitelre, a szimuláció futása valós időben történik lépésenként

### Biztonság
- az adatok bizonsága megfelelő használat mellett garantált