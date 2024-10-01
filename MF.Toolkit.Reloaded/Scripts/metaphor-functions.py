#Add names to functions from a dumped function.json.
#Also generates a functions.json.def for generating MetaphorLibrary source.
#@author RyoTune
#@category Metaphor
#@keybinding 
#@menupath 
#@toolbar 

from ghidra.program.model.symbol import SourceType

from javax.swing import JFileChooser
import json

base_address = currentProgram.getMinAddress()
listing = currentProgram.getListing()

class FunctionDefinition:
  def __init__(self, name, ret):
    self._name = name
    self._ret = ret
    self._args = []

  def addArg(self, arg):
    self._args.append(arg)

  def to_dict(self):
    return {
      'name': self._name,
      'ret': self._ret,
      'args': self._args
    }

def select_file():
  chooser = JFileChooser()
  chooser.setDialogTitle('Select functions.json')
  result = chooser.showOpenDialog(None)
  if result == JFileChooser.APPROVE_OPTION:
    return chooser.getSelectedFile().getAbsolutePath()
  else:
    return None

def deserialize_file(file_path):
  with open(file_path, 'r') as file:
    try:
      return json.load(file)
    except any:
      return None

def serialize_file(file_path, dict):
  with open(file_path, 'w') as file:
    try:
      return json.dump(dict, file)
    except any:
      return None

def process_function(ea, name):
  func = listing.getFunctionAt(ea)
  
  if func != None:
    func.setName(name, SourceType.USER_DEFINED)
  else:
    func = createFunction(ea, name)
  
  sig = func.getSignature()
  params = sig.getArguments()
  ret = sig.getReturnType().getName()
  funcDef = FunctionDefinition(name, ret)
  for idx, paramType in enumerate(params):
    funcDef.addArg(paramType.getDataType().getName())

  return funcDef.to_dict();

def apply_functions(functions, outfile):
  func_defs = []
  for func in functions:
    name = func['Name']
    offset = int(func['Offset'], 16)
    address = base_address.add(offset)
    new_func = process_function(address, name)
    func_defs.append(new_func)
    print(name + ': ' + str(address))
  serialize_file(outfile, func_defs);

def run():
  functionsFile = select_file()
  if (functionsFile == None):
    return
  
  functions = deserialize_file(functionsFile)
  if (functions is not None):
    apply_functions(functions, functionsFile + '.def');

run()
