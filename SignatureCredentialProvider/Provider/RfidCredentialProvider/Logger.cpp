#include "common.h"
#include <stdio.h>
#include <time.h>
#include <iostream>
#include <string>
#include "Logger.h"

void Log(const char *toWrite){
	FILE * pFile;
	/*
	time_t     now;
	struct tm timeinfo;
	char       buf[80];
 
	now = time(NULL);
 


	localtime_s(&timeinfo, *timeinfo);
	strftime(buf, sizeof(buf), "%a %Y-%m-%d %H:%M:%S", ts);*/

	char str[70];
	time_t rawtime;
	//struct tm * timeinfo;
	struct tm timeinfo;
	time(&rawtime);
	//timeinfo = localtime(&rawtime);
	//localtime_s (timeinfo,&rawtime);
	localtime_s(&timeinfo, &rawtime);
	//strftime(str ,100 , "It is %Y-%m-%d %Z %X %x\n",timeinfo);
	strftime(str, 100, "It is %Y-%m-%d %Z %X %x\n", &timeinfo);

	//pFile = fopen_s ("C:\\SignatureProvider.log.txt","a");
	fopen_s(&pFile, "C:\\SignatureProviderRFID.log.txt", "a");
	fprintf(pFile, "%s\t%s\n", str, toWrite);
	fclose(pFile);
}