from datetime import datetime
import pandas as pd
import csv
import json
import os

def export():
    excel_file = './config_chesstactic.xlsx'
    all_sheets = pd.read_excel(excel_file, sheet_name=None)
    sheets = all_sheets.keys()

    for sheet_name in sheets:
        if sheet_name[:5].upper() == 'SHEET':
            continue
        
        sheet = pd.read_excel(excel_file, sheet_name=sheet_name)
        sheet.to_csv("./%s.csv" % sheet_name, index=False)

def read(path):
    file = open('./' + path)
    csvreader = csv.reader(file)
    header = next(csvreader)

    return file, csvreader

def write(json_objects, path):
    sz = json.dumps(json_objects, indent=4)
    with open('../config/' + path, 'w') as f:
        f.write(sz)
#--------------------------------------------------------------
def make():
    def get_json(arr):
        pos = arr[3].split(',')
        ability = json.loads(arr[5])
        item = json.loads(arr[6])
        j = {
            "side": int(arr[0]),
            "id": int(arr[1]),
            "name": arr[2],
            "position": {
                "x": int(pos[0]),
                "y": int(pos[1]),
                "z": int(pos[2])
            },
            "movingType": int(arr[4]),
            "ability": ability,
            "item": item
        }
        return j, arr[0]
    
    def get_json_info(arr):
        j = {
            "name": arr[1],
            "attackTactic": int(arr[2]),
            "defenceTactic": int(arr[3])
        }
        return j, arr[0]

    json_object = {}

    file, csvreader = read('chesstactic_soldier.csv')
    for row in csvreader:
        j, side = get_json(row)
        if side not in json_object:
            json_object[side] = {
                "info": {},
                "soldiers": []
            }

        json_object[side]["soldiers"].append(j)    
    file.close()

    file, csvreader = read('chesstactic_tactic.csv')
    for row in csvreader:
        j, side = get_json_info(row)
        json_object[side]["info"] = j
    file.close()

    write(json_object, 'battle_chesstactic.json')
#--------------------------------------------------------------     
export()
print(datetime.now(), 'exported')
make()
print(datetime.now(), 'wrote config')
os.remove('./chesstactic_soldier.csv')
os.remove('./chesstactic_tactic.csv')
print(datetime.now(), 'removed csv files')