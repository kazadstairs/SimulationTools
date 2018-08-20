SCHEDNAMES <- c("Rolling Machine Assignment", "Random", "GreedyLoadBalancing")
INSTANCES <- c("Pinedo",
               "30j-15r-4m.ms",
               "30j-15r-8m.ms",
               "30j-30r-4m.ms",
               "30j-30r-4m.ms",
               "30j-75r-4m.ms",
               "30j-75r-4m.ms",
               "100j-50r-6m.ms",
               "100j-50r-12m.ms",
               "100j-100r-6m.ms",
               "100j-100r-12m.ms",
               "100j-250r-6m.ms",
               "100j-250r-12m.ms")
NRUNS <- "1000"

RM.ID <- 1
QM.ID <- 2

###
########################## Libraries #########################################
###
library(ggplot2)
library(plyr)
library(dplyr)

###
########################## FUNCTIONS #########################################
###
GetPathToOutput <- function(InstanceName,ScheduleName,Nruns,FileType){
	# BasePath <- "C:\\Users\\Gebruiker\\Documents\\UU\\MSc Thesis\\Code\\OutPut\\" #laptop
  BasePath <- "C:\\Users\\3496724\\Source\\Repos\\SimulationTools\\Results\\RMs\\" #UU pc
	PathComponents <- c(BasePath,
				  "Instance_", InstanceName,
				  "_Schedule_", ScheduleName,
				  "_Runs_", Nruns,
				  "_",FileType,
				  ".txt")
	FilePath <- paste(PathComponents, collapse="")
	return(FilePath)
}


GetSimSettings <- function(InstanceName,ScheduleName,Nruns){
	path <- GetPathToOutput(InstanceName,ScheduleName,Nruns,"SimSettings")
	return(read.csv2(file = path,header = FALSE))
}
GetQMs <- function(InstanceName,ScheduleName,Nruns){
	path <- GetPathToOutput(InstanceName,ScheduleName,Nruns,"QMs")
	return(read.csv2(file = path, header = FALSE))
}

GetInstanceName <- function(){
	return(SimSettings[1,])
}

GetScheduleName <- function(){
	return(SimSettings[2,])
}

GetRMName <- function(){
	f <- levels(GetSimSettings(INSTANCE,SCHEDNAMES[1],NRUNS)[2+2*RM.ID,])[6]
	return(f)
}

QMNames <- c("Cmax","Linear Start Delay","Start Punctuality","TODO: Completion Punctuality")
GetQMName <- function(){
	return(QMNames[QM.ID])
}

GetRM <- function(SimSettings){
	f <- SimSettings[3+2*RM.ID,]
	return( as.numeric(levels(f))[f] )
}

GetQM <- function(QMData){
	return(QMData[,1+QM.ID])
}



GetCmaxColumn <- function(QMData){
	return(QMData[,2])
}

GetLinearDelayColumn <- function(){
	return(QMData[,3])
}

GetStartPunctualityColumn <- function(){
	return(QMData[,4])
}

GetCompletionPunctualiyColumn <- function(){
	return(QMData[,5])
}
####




BuildDfFor <- function(InstanceName,ScheduleName,Nruns){ 
	rmvals <- GetRM(GetSimSettings(InstanceName,ScheduleName,Nruns))
	cmax <- GetQM(GetQMs(InstanceName,ScheduleName,Nruns))
	df <- data.frame( RM = rmvals, Cmax = cmax)
	df.summary <- df %>% group_by(RM) %>% summarize(ymin = mean(cmax) - sd(cmax), ymax = mean(cmax) + sd(cmax),ymean = mean(cmax))
	df.summary$PointLabel <- abbreviate(ScheduleName)
	return( df.summary )
}

# build data frame:
BuildPlot <- function(cScheduleNames,InstanceName,Nruns)
{

	
	##
	
	dflist <- lapply(cScheduleNames,FUN = BuildDfFor, InstanceName = InstanceName, Nruns = Nruns)
	dfplotinfo <- rbind.fill(dflist)
	
	return (dfplotinfo)
}

#
#
######## ACTUAL WORK ##########################################
#
#


#plot.df <- BuildPlot(SCHEDNAMES,INSTANCE,NRUNS)
#errorBarWidth <- max(plot.df$RM) / (4 * length(unique(plot.df$RM)))
#p <- ggplot(plot.df, aes(x = RM, y = ymean, label= PointLabel)) + geom_point(size = 2) + geom_errorbar(aes(ymin = ymin, ymax = ymax, width = errorBarWidth))+geom_text(aes(label=PointLabel),hjust=0, vjust=0)
#p + labs(x = paste(c("Schedule ",abbreviate(GetRMName())," score"),collapse=''), y = GetQMName())

########### plot from one big data file ############
myDF <- read.csv2("C:/Users/3496724/Source/Repos/SimulationTools/Results/RMs/allresults.txt")
myDF.plot <- myDF %>% 
  group_by(Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
  summarize(FS = mean(FS),Cmaxsd = sd(Cmax),Cmax=mean(Cmax))

p <- ggplot(myDF.plot,aes(x=FS,y=Cmax,colour=Schedule.StartTimeType,shape=Schedule.StartTimeType)) 
p <- p + geom_point() 
p <- p + geom_errorbar(aes(ymin=Cmax-Cmaxsd,ymax=Cmax+Cmaxsd))
p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,2000)) 
p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,500))

p


MakeAllPlots <- function()
{
  RMs <- c("FS","BFS","UFS")
  QMs <- c("Cmax","LinearStartDelay","Start.Punctuality","Finish.Punctuality")
  
  for(Rm in RMs)
  {
    for(Qm in QMs)
    {
      MakePlot(Rm,Qm)
    }
    
  }
  
}
MakeAllPlots()

MakePlot <- function(string.RM, string.QM)
{
  RMsym <- sym(string.RM)
  QMsym <- sym(string.QM)
  library(ggplot2)
  #library(tidyverse)
  
  myDF.plot <- myDF %>% 
    group_by(Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QMsd = sd(!!QMsym),QM=mean(!!QMsym))
  
 # names(myDF.plot)[3] <- string.RM
#  names(myDF.plot)[4] <- paste(string.QM,"sd",sep="")
#  names(myDF.plot)[5] <- string.QM
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.StartTimeType,shape=Schedule.StartTimeType)) 
  p <- p + geom_point() 
  p <- p + geom_errorbar(aes(ymin=QM-QMsd,ymax=QM+QMsd))
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(string.QM)
  p <- p + theme(legend.position = "top")
  
  fileName <- paste("C:\\Users\\3496724\\Source\\Repos\\SimulationTools\\Results\\RMs\\",string.RM,"_vs_",string.QM,".pdf",sep="")
  ggsave(filename = fileName,plot = p)
  
  print(p)
  
}


