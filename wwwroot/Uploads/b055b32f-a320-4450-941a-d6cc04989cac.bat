@echo off

set SERVER_IP=10.5.100.7
set SERVER_USER=developpeur
set SERVER_PASS=TON_MDP
set SOURCE=C:\Users\PRMADA-23943\Documents\jobs\Anosy\rh\Application\new_contrat\contrat\front\parera\build\*
set DEST=/etc/logistique/rh/front

call npm run build

echo === Copie vers le serveur Ubuntu ===
scp -r "%SOURCE%" %SERVER_USER%@%SERVER_IP%:%DEST%



echo === Déploiement terminé ===
pause
